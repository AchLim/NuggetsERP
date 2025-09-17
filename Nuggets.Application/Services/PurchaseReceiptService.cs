using Nuggets.Application.Common;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Application.Common.Services;
using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Services;

public sealed class PurchaseReceiptService(
    IPurchaseReceiptRepository repo,
    IStockMovementRepository stockRepo,
    IInventoryService inventoryService,
    IChartOfAccountRepository coaRepo,
    IJournalEntryRepository journalRepo,
    IJournalEntryService journalService
) : IPurchaseReceiptService
{
    public async Task<Result<PagedResult<PurchaseReceiptListDto>>> GetPagedAsync(int page, int pageSize)
    {
        var (items, total) = await repo.GetPagedAsync(page, pageSize);
        var list = items.Select(ToListDto).ToList();
        return Result<PagedResult<PurchaseReceiptListDto>>.Ok(new PagedResult<PurchaseReceiptListDto>(list, total, page, pageSize));
    }

    public async Task<Result<IReadOnlyList<PurchaseReceiptListDto>>> GetAllAsync()
    {
        var items = await repo.GetAllAsync();
        return Result<IReadOnlyList<PurchaseReceiptListDto>>.Ok(items.Select(ToListDto).ToList());
    }

    public async Task<Result<PurchaseReceiptReadDto>> GetByIdAsync(Guid id)
    {
        var e = await repo.GetByIdAsync(id);
        return e is not null ? Result<PurchaseReceiptReadDto>.Ok(ToReadDto(e)) : Result<PurchaseReceiptReadDto>.Err("Purchase receipt not found", "NOT_FOUND");
    }

    public async Task<Result<PurchaseReceiptReadDto>> CreateAsync(PurchaseReceiptCreateDto dto)
    {
        if (dto.Lines.Count == 0) return Result<PurchaseReceiptReadDto>.Err("At least one line required", "VALIDATION_ERROR");
        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var draftLabel = $"Draft PR *{DateTime.UtcNow:yyyyMMddHHmmss}";

            var ent = new PurchaseReceipt
            {
                VendorId = dto.VendorId,
                ReceiptNumber = draftLabel, 
                ReceiptDate = dto.ReceiptDate,
                Status = PurchaseReceiptStatus.Draft,
                Method = dto.Method,
                Lines = dto.Lines.Select(l => new PurchaseReceiptLine
                {
                    ProductId = l.ProductId,
                    UomId = l.UomId,
                    Quantity = l.Quantity,
                    UnitCost = l.UnitCost
                }).ToList()
            };

            await repo.AddAsync(ent);
            
            await tx.CommitAsync();
            return Result<PurchaseReceiptReadDto>.Ok(ToReadDto(ent));
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<PurchaseReceiptReadDto>.Err($"Failed to create purchase receipt: {ex.Message}", "DB_ERROR");
        }
    }

    public async Task<Result<PurchaseReceiptReadDto>> UpdateAsync(Guid id, PurchaseReceiptUpdateDto dto)
    {
        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing is null) return Result<PurchaseReceiptReadDto>.Err("Purchase receipt not found", "NOT_FOUND");
            
            existing.VendorId = dto.VendorId;
            existing.ReceiptDate = dto.ReceiptDate;
            existing.Method = dto.Method;

            existing.Lines.Clear();
            foreach (var l in dto.Lines)
            {
                existing.Lines.Add(new PurchaseReceiptLine
                {
                    ProductId = l.ProductId,
                    UomId = l.UomId,
                    Quantity = l.Quantity,
                    UnitCost = l.UnitCost
                });
            }
            
            if (dto.Status == PurchaseReceiptStatus.Received && 
                (string.IsNullOrEmpty(existing.ReceiptNumber) || existing.ReceiptNumber.StartsWith("Draft PR")))
            {
                // Generate auto number
                var nextNumber = await repo.GetNextSequenceValueAsync("purchase_receipt_number_seq");
                existing.ReceiptNumber = $"PR/{dto.ReceiptDate.Year}/{nextNumber:000000}";
            }

            if (dto.Status == PurchaseReceiptStatus.Received && existing.Status != PurchaseReceiptStatus.Received)
            {
                // Delegate stock + journal
                await inventoryService.ApplyPurchaseReceiptAsync(existing.Id);
            }
            else if (dto.Status == PurchaseReceiptStatus.Cancelled && existing.Status == PurchaseReceiptStatus.Received)
            {
                await inventoryService.RevertPurchaseReceiptAsync(existing.Id);
            }

            existing.Status = dto.Status;
            await repo.UpdateAsync(existing);

            await tx.CommitAsync();
            return Result<PurchaseReceiptReadDto>.Ok(ToReadDto(existing));
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<PurchaseReceiptReadDto>.Err($"Failed to update: {ex.Message}", "DB_ERROR");
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing is null) return Result<bool>.Err("Receipt not found", "NOT_FOUND");
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

    private static PurchaseReceiptListDto ToListDto(PurchaseReceipt r) =>
        new(r.Id, r.VendorId, r.Vendor.Name, r.ReceiptNumber, r.ReceiptDate, r.Status, r.Method, r.Lines.Sum(l => l.Quantity * l.UnitCost));

    private static PurchaseReceiptReadDto ToReadDto(PurchaseReceipt r) =>
        new(r.Id, r.VendorId, r.Vendor?.Name, r.ReceiptNumber, r.ReceiptDate, r.Status, r.Method,
            r.Lines.Select(l => new PurchaseReceiptLineReadDto(l.Id, l.ProductId, l.Product?.Name, l.UomId, l.Quantity, l.UnitCost, l.Quantity * l.UnitCost)).ToList());
}
