using Nuggets.Application.Common;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Application.Common.Services;
using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Services;

public sealed class VendorBillService(
    IVendorBillRepository repo,
    IPurchaseReceiptRepository purchaseReceiptRepo,
    IJournalEntryRepository journalRepo,
    IChartOfAccountRepository coaRepo,
    IJournalEntryService journalService
) : IVendorBillService
{
    public async Task<Result<PagedResult<VendorBillListDto>>> GetPagedAsync(int page, int pageSize)
    {
        var (items, total) = await repo.GetPagedAsync(page, pageSize);
        var list = items.Select(ToListDto).ToList();
        return Result<PagedResult<VendorBillListDto>>.Ok(new PagedResult<VendorBillListDto>(list, total, page, pageSize));
    }

    public async Task<Result<IReadOnlyList<VendorBillListDto>>> GetAllAsync()
    {
        var items = await repo.GetAllAsync();
        return Result<IReadOnlyList<VendorBillListDto>>.Ok(items.Select(ToListDto).ToList());
    }

    public async Task<Result<VendorBillReadDto>> GetByIdAsync(Guid id)
    {
        var e = await repo.GetByIdAsync(id);
        return e is not null ? Result<VendorBillReadDto>.Ok(ToReadDto(e)) : Result<VendorBillReadDto>.Err("Vendor bill not found", "NOT_FOUND");
    }

    public async Task<Result<VendorBillReadDto>> CreateAsync(VendorBillCreateDto dto)
    {
        if (dto.Lines.Count == 0) return Result<VendorBillReadDto>.Err("At least one line required", "VALIDATION_ERROR");
        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var draftLabel = $"Draft VB *{DateTime.UtcNow:yyyyMMddHHmmss}";

            var ent = new VendorBill
            {
                VendorId = dto.VendorId,
                BillNumber = draftLabel,
                BillDate = dto.BillDate,
                Status = VendorBillStatus.Draft,
                Lines = dto.Lines.Select(l => new VendorBillLine
                {
                    ProductId = l.ProductId,
                    UomId = l.UomId,
                    Quantity = l.Quantity,
                    UnitCost = l.UnitCost
                }).ToList()
            };

            await repo.AddAsync(ent);

            await tx.CommitAsync();
            return Result<VendorBillReadDto>.Ok(ToReadDto(ent));
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<VendorBillReadDto>.Err($"Failed to create vendor bill: {ex.Message}", "DB_ERROR");
        }
    }

    public async Task<Result<VendorBillReadDto>> UpdateAsync(Guid id, VendorBillUpdateDto dto)
    {
        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing is null) return Result<VendorBillReadDto>.Err("Vendor bill not found", "NOT_FOUND");

            // ========== Handle Posting ==========
            if (dto.Status == VendorBillStatus.Posted && 
                (string.IsNullOrEmpty(existing.BillNumber) || existing.BillNumber.StartsWith("Draft VB")))
            {
                // Generate auto number
                var nextNumber = await repo.GetNextSequenceValueAsync("vendor_bill_number_seq");
                existing.BillNumber = $"VB/{dto.BillDate.Year}/{nextNumber:000000}";
            }
            
            // Journal Entry: Dr GRNI / Cr AP
            if (dto.Status == VendorBillStatus.Posted && existing.Status != VendorBillStatus.Posted)
            {
                var invAcc = await coaRepo.GetInventoryAccountAsync();
                var apAcc  = await coaRepo.GetPayableAccountAsync();

                var total = existing.Lines.Sum(l => l.Quantity * l.UnitCost);

                var je = await journalService.PostAsync(
                    $"Vendor Bill {existing.BillNumber} for PO {existing.PurchaseOrder?.OrderNumber}",
                    dto.BillDate,
                    new[]
                    {
                        (invAcc, total, 0m),  // Dr Inventory
                        (apAcc, 0m, total)    // Cr Accounts Payable
                    });
                existing.JournalEntryId = je.Id;
            }
            else if (dto.Status == VendorBillStatus.Cancelled && existing.Status == VendorBillStatus.Posted)
            {
                if (existing.JournalEntryId.HasValue)
                {
                    var je = await journalRepo.GetByIdAsync(existing.JournalEntryId.Value);
                    if (je != null) await journalService.ReverseAsync(je, "Vendor Bill cancelled");
                }
            }
            
            existing.VendorId = dto.VendorId;
            existing.BillDate = dto.BillDate;
            existing.Status = dto.Status;

            existing.Lines.Clear();
            foreach (var l in dto.Lines)
            {
                existing.Lines.Add(new VendorBillLine
                {
                    ProductId = l.ProductId,
                    UomId = l.UomId,
                    Quantity = l.Quantity,
                    UnitCost = l.UnitCost
                });
            }

            await repo.UpdateAsync(existing);
            
            await tx.CommitAsync();
            return Result<VendorBillReadDto>.Ok(ToReadDto(existing));
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<VendorBillReadDto>.Err($"Failed to update vendor bill: {ex.Message}", "DB_ERROR");
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing is null) return Result<bool>.Err("Vendor bill not found", "NOT_FOUND");
            await repo.DeleteAsync(existing);
            await tx.CommitAsync();
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<bool>.Err($"Failed to delete: {ex.Message}", "DB_ERROR");
        }
    }

    private static VendorBillListDto ToListDto(VendorBill v) =>
        new(v.Id, v.VendorId, v.Vendor?.Name, v.BillDate, v.Status.ToString(), v.Lines.Sum(l => l.Quantity * l.UnitCost));

    private static VendorBillReadDto ToReadDto(VendorBill v) =>
        new(v.Id, v.VendorId, v.Vendor?.Name, v.BillDate, v.Status, v.Lines.Select(l => new VendorBillLineReadDto(l.Id, l.ProductId, l.Product?.Name, l.UomId, l.Quantity, l.UnitCost, l.Quantity * l.UnitCost)).ToList());
}
