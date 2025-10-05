using Nuggets.Application.Common;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Application.Common.Services;
using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Services;

public sealed class VendorBillService(
    IVendorBillRepository repo,
    IPurchaseReceiptRepository purchaseReceiptRepo,
    IPurchaseOrderRepository poRepo,
    IInventoryService inventoryService,
    IJournalEntryRepository journalRepo,
    IChartOfAccountRepository coaRepo,
    IJournalEntryService journalService
) : IVendorBillService
{
    public async Task<Result<PagedResult<VendorBillListDto>>> GetPagedAsync(int page, int pageSize, Guid? purchaseOrderId = null)
    {
        var result = purchaseOrderId.HasValue
            ? await repo.GetPagedAsync(page, pageSize, repo.Query().Where(vb => vb.PurchaseOrderId == purchaseOrderId.Value))
            : await repo.GetPagedAsync(page, pageSize);

        var list = result.Items.Select(ToListDto).ToList();
        return Result<PagedResult<VendorBillListDto>>.Ok(
            new PagedResult<VendorBillListDto>(list, result.TotalCount, page, pageSize));
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
        if (dto.PurchaseOrderId.HasValue)
        {
            var po = await poRepo.GetWithLinesAndBillsAsync(dto.PurchaseOrderId.Value);
            if (po == null)
                return Result<VendorBillReadDto>.Err("Purchase Order not found.", "NOT_FOUND");

            var receivedQty = po.GoodsReceiptNotes
                .Where(grn => grn.Status is GoodsReceiptNoteStatus.Received)
                .SelectMany(grn => grn.Lines)
                .Sum(l => l.Quantity);

            var alreadyBilled = po.VendorBills
                .Where(vb => vb.Status is VendorBillStatus.Posted or VendorBillStatus.Paid)
                .SelectMany(vb => vb.Lines)
                .Sum(l => l.Quantity);

            var newQty = dto.Lines.Sum(l => l.Quantity);

            if (receivedQty == 0)
                return Result<VendorBillReadDto>.Err("Cannot create bill before receipt is recorded.", "VALIDATION_ERROR");

            if (alreadyBilled + newQty > receivedQty)
                return Result<VendorBillReadDto>.Err("Cannot bill more than received.", "VALIDATION_ERROR");
        }
        
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
                PurchaseOrderId = dto.PurchaseOrderId,
                Lines = dto.Lines.Select(l => new VendorBillLine
                {
                    ProductId = l.ProductId,
                    UomId = l.UomId,
                    Quantity = l.Quantity,
                    UnitCost = l.UnitCost,
                    DiscountPercent = l.DiscountPercent
                }).ToList()
            };

            await repo.AddAsync(ent);

            await tx.CommitAsync();
            return await GetByIdAsync(ent.Id);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<VendorBillReadDto>.Err($"Failed to create vendor bill: {ex.Message}", "DB_ERROR");
        }
    }

    public async Task<Result<VendorBillReadDto>> UpdateAsync(Guid id, VendorBillUpdateDto dto)
    {
        var existing = await repo.GetByIdWithLinesAsync(id);
        if (existing is null)
            return Result<VendorBillReadDto>.Err("Vendor Bill not found.", "NOT_FOUND");

        if (existing.PurchaseOrderId.HasValue)
        {
            var po = await poRepo.GetWithLinesAndBillsAsync(existing.PurchaseOrderId.Value);

            if (po is null)
                return Result<VendorBillReadDto>.Err("Purchase Order not found.", "NOT_FOUND");
            var receivedQty = po.GoodsReceiptNotes
                .Where(grn => grn.Status == GoodsReceiptNoteStatus.Received)
                .SelectMany(grn => grn.Lines)
                .Sum(l => l.Quantity);

            var alreadyBilled = po.VendorBills
                .Where(vb => (vb.Status is VendorBillStatus.Posted or VendorBillStatus.Paid) && vb.Id != existing.Id)
                .SelectMany(vb => vb.Lines)
                .Sum(l => l.Quantity);

            var newQty = dto.Lines.Sum(l => l.Quantity);

            if (alreadyBilled + newQty > receivedQty)
                return Result<VendorBillReadDto>.Err("Cannot bill more than received.", "VALIDATION_ERROR");
        }

        
        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            existing.VendorId = dto.VendorId;
            existing.BillDate = dto.BillDate;
            existing.PurchaseOrderId = dto.PurchaseOrderId;

            existing.Lines.Clear();
            foreach (var l in dto.Lines)
            {
                existing.Lines.Add(new VendorBillLine
                {
                    ProductId = l.ProductId,
                    UomId = l.UomId,
                    Quantity = l.Quantity,
                    UnitCost = l.UnitCost,
                    DiscountPercent = l.DiscountPercent
                });
            }
            
            if (dto.Status == VendorBillStatus.Posted && 
                (string.IsNullOrEmpty(existing.BillNumber) || existing.BillNumber.StartsWith("Draft VB")))
            {
                // Generate auto number
                var nextNumber = await repo.GetNextSequenceValueAsync("vendor_bill_number_seq", tx);
                existing.BillNumber = $"VB/{dto.BillDate.Year}/{nextNumber:000000}";
            }
            
            // Handle posting: Create Journal Entries
            if (dto.Status == VendorBillStatus.Posted && existing.Status != VendorBillStatus.Posted)
            {
                // Apply inventory and average cost update
                await inventoryService.ApplyVendorBillAsync(existing.Id);
            }
            // --- Handle Cancellation ---
            else if (dto.Status == VendorBillStatus.Cancelled && existing.Status == VendorBillStatus.Posted)
            {
                await inventoryService.RevertVendorBillAsync(existing.Id);
            }

            existing.Status = dto.Status;

            await repo.UpdateAsync(existing);
            await tx.CommitAsync();
            return await GetByIdAsync(existing.Id);
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

            if (!(string.IsNullOrEmpty(existing.BillNumber) || existing.BillNumber.StartsWith("Draft VB")))
            {
                return Result<bool>.Err("You are not allowed to delete a transaction with existing number.", "VALIDATION_ERROR");
            }
            
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
        new(v.Id, v.BillNumber, v.VendorId, v.Vendor?.Name, v.BillDate, v.PurchaseOrderId, v.PurchaseOrder?.OrderNumber, v.Status.ToString(), v.Lines.Sum(l => l.LineTotal));

    private static VendorBillReadDto ToReadDto(VendorBill v)
    {
        var paidAmount = v.VendorPayments
            .Where(vp => vp.Status is VendorPaymentStatus.Posted)
            .Sum(l => l.Amount);

        var billedAmount = v.Lines.Sum(l => l.LineTotal);

        return new(
            v.Id,
            v.BillNumber,
            v.VendorId,
            v.Vendor?.Name,
            v.BillDate,
            v.PurchaseOrderId,
            v.PurchaseOrder?.OrderNumber,
            v.Status,
            v.Lines.Select(l => new VendorBillLineReadDto(l.Id, l.ProductId, l.Product?.Name, l.UomId, l.Quantity,
                l.UnitCost, l.DiscountPercent, l.LineTotal)).ToList(),
            BilledAmount: billedAmount,
            PaidAmount: paidAmount
        );
    }
}
