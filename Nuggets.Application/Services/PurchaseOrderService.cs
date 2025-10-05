using Nuggets.Application.Common;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Application.Common.Services;
using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Services;

public sealed class PurchaseOrderService(IPurchaseOrderRepository repo) : IPurchaseOrderService
{
    public async Task<Result<PagedResult<PurchaseOrderListDto>>> GetPagedAsync(int page, int pageSize)
    {
        var (items, total) = await repo.GetPagedAsync(page, pageSize);
        var list = items.Select(ToListDto).ToList();
        return Result<PagedResult<PurchaseOrderListDto>>.Ok(new PagedResult<PurchaseOrderListDto>(list, total, page, pageSize));
    }

    public async Task<Result<IReadOnlyList<PurchaseOrderListDto>>> GetAllAsync()
    {
        var items = await repo.GetAllAsync();
        return Result<IReadOnlyList<PurchaseOrderListDto>>.Ok(items.Select(ToListDto).ToList());
    }

    public async Task<Result<PurchaseOrderReadDto>> GetByIdAsync(Guid id)
    {
        var ent = await repo.GetByIdAsync(id);
        return ent is not null ? Result<PurchaseOrderReadDto>.Ok(ToReadDto(ent)) : Result<PurchaseOrderReadDto>.Err("Purchase order not found", "NOT_FOUND");
    }

    public async Task<Result<PurchaseOrderReadDto>> CreateAsync(PurchaseOrderCreateDto dto)
    {
        if (dto.Lines.Count == 0) return Result<PurchaseOrderReadDto>.Err("At least one line required", "VALIDATION_ERROR");
        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var draftLabel = $"Draft PO *{DateTime.UtcNow:yyyyMMddHHmmss}";
            
            var ent = new PurchaseOrder
            {
                VendorId = dto.VendorId,
                OrderDate = dto.OrderDate,
                OrderNumber = draftLabel,
                Status = PurchaseOrderStatus.Draft,
                Lines = dto.Lines.Select(l => new PurchaseOrderLine
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
            return Result<PurchaseOrderReadDto>.Ok(ToReadDto(ent));
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<PurchaseOrderReadDto>.Err($"Failed to create purchase order: {ex.Message}", "DB_ERROR");
        }
    }

    public async Task<Result<PurchaseOrderReadDto>> UpdateAsync(Guid id, PurchaseOrderUpdateDto dto)
    {
        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing is null) return Result<PurchaseOrderReadDto>.Err("Purchase order not found", "NOT_FOUND");

            if (dto.Status == PurchaseOrderStatus.Approved && 
                (string.IsNullOrEmpty(existing.OrderNumber) || existing.OrderNumber.StartsWith("Draft PO")))
            {
                // Generate auto number
                var nextNumber = await repo.GetNextSequenceValueAsync("purchase_order_number_seq", tx);
                existing.OrderNumber = $"PO/{dto.OrderDate.Year}/{nextNumber:000000}";
            }
            
            existing.VendorId = dto.VendorId;
            existing.OrderDate = dto.OrderDate;
            existing.Status = dto.Status;

            existing.Lines.Clear();
            foreach (var l in dto.Lines)
            {
                existing.Lines.Add(new PurchaseOrderLine
                {
                    ProductId = l.ProductId,
                    UomId = l.UomId,
                    Quantity = l.Quantity,
                    UnitCost = l.UnitCost,
                    DiscountPercent = l.DiscountPercent
                });
            }

            await repo.UpdateAsync(existing);
            await tx.CommitAsync();
            return Result<PurchaseOrderReadDto>.Ok(ToReadDto(existing));
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<PurchaseOrderReadDto>.Err($"Failed to update: {ex.Message}", "DB_ERROR");
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing is null) return Result<bool>.Err("Purchase order not found", "NOT_FOUND");
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

    private static PurchaseOrderListDto ToListDto(PurchaseOrder p) =>
        new(p.Id, p.VendorId, p.Vendor?.Name, p.OrderNumber, p.OrderDate, p.Status.ToString(), p.Lines.Sum(l => l.LineTotal));

    private static PurchaseOrderReadDto ToReadDto(PurchaseOrder po)
    {
        var orderedQty = po.Lines.Sum(l => l.Quantity);

        var receivedQty = po.GoodsReceiptNotes
            .Where(grn => grn.Status == GoodsReceiptNoteStatus.Received)
            .SelectMany(grn => grn.Lines)
            .Sum(l => l.Quantity);

        var billedQty = po.VendorBills
            .Where(vb => vb.Status == VendorBillStatus.Posted || vb.Status == VendorBillStatus.Paid)
            .SelectMany(vb => vb.Lines)
            .Sum(l => l.Quantity);

        return new PurchaseOrderReadDto(
            po.Id,
            po.VendorId,
            po.Vendor?.Name,
            po.OrderNumber,
            po.OrderDate,
            po.Status,
            po.Lines.Select(l =>
            {
                var receivedForLine = po.GoodsReceiptNotes
                    .Where(grn => grn.Status is GoodsReceiptNoteStatus.Received)
                    .SelectMany(grn => grn.Lines)
                    .Where(gl => gl.ProductId == l.ProductId && gl.UomId == l.UomId)
                    .Sum(gl => gl.Quantity);

                var remaining = l.Quantity - receivedForLine;

                return new PurchaseOrderLineReadDto(
                    l.Id,
                    l.ProductId,
                    l.Product?.Name,
                    l.UomId,
                    l.Quantity,
                    l.UnitCost,
                    l.DiscountPercent,
                    l.LineTotal,
                    remaining
                );
            }).ToList(),
            orderedQty,
            receivedQty,
            billedQty
        );
    }
}
