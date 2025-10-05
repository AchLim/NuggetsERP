using Nuggets.Application.Common;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Application.Common.Services;
using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Services;

public sealed class GoodsReceiptNoteService(
    IGoodsReceiptNoteRepository repo,
    IProductRepository productRepo,
    IInventoryService inventoryService,
    IPurchaseOrderRepository poRepo,
    IUomService uomService)
    : IGoodsReceiptNoteService
{
    public async Task<Result<PagedResult<GoodsReceiptNoteListDto>>> GetPagedAsync(int page, int pageSize, Guid? purchaseOrderId = null)
    {
        (IReadOnlyList<GoodsReceiptNote> items, int totalCount) result;
        if (purchaseOrderId.HasValue)
        {
            var query = repo.Query().Where(grn => grn.PurchaseOrderId == purchaseOrderId.Value);
            result = await repo.GetPagedAsync(page, pageSize, query);
        }
        else
        {
            result = await repo.GetPagedAsync(page, pageSize);
        }
        

        var list = result.items.Select(ToListDto).ToList();

        return Result<PagedResult<GoodsReceiptNoteListDto>>.Ok(
            new PagedResult<GoodsReceiptNoteListDto>(list, result.totalCount, page, pageSize));
    }

    public async Task<Result<GoodsReceiptNoteReadDto>> GetByIdAsync(Guid id)
    {
        var dn = await repo.GetByIdWithLinesAsync(id);
        if (dn == null)
            return Result<GoodsReceiptNoteReadDto>.Err("GRN not found.", "NOT_FOUND");

        return Result<GoodsReceiptNoteReadDto>.Ok(ToReadDto(dn));
    }

    public async Task<Result<GoodsReceiptNoteReadDto>> CreateAsync(GoodsReceiptNoteCreateDto dto)
    {
        var po = await poRepo.GetWithLinesAndGrnsAsync(dto.PurchaseOrderId);
        if (po is null) return Result<GoodsReceiptNoteReadDto>.Err("Purchase Order not found", "NOT_FOUND");

        // Business validation
        var orderedQty = po.Lines.Sum(l => l.Quantity);
        var receivedQty = po.GoodsReceiptNotes
            .Where(grn => grn.Status is GoodsReceiptNoteStatus.Received)
            .SelectMany(grn => grn.Lines)
            .Sum(l => l.Quantity);

        var newQty = dto.Lines.Sum(l => l.Quantity);

        if (receivedQty + newQty > orderedQty)
            return Result<GoodsReceiptNoteReadDto>.Err("Cannot receive more than PO ordered quantity", "VALIDATION_ERROR");

        var draftLabel = $"Draft GRN *{DateTime.UtcNow:yyyyMMddHHmmss}";

        var dn = new GoodsReceiptNote
        {
            GRNNumber = draftLabel,
            PurchaseOrderId = dto.PurchaseOrderId,
            ReceiptDate = dto.ReceiptDate,
            Status = GoodsReceiptNoteStatus.Draft,
            Lines = dto.Lines.Select(lineDto => new GoodsReceiptNoteLine
            {
                ProductId = lineDto.ProductId,
                UomId = lineDto.UomId,
                Quantity = lineDto.Quantity
            }).ToList()
        };

        await repo.AddAsync(dn);
        return await GetByIdAsync(dn.Id);
    }

    public async Task<Result<GoodsReceiptNoteReadDto>> UpdateAsync(Guid id, GoodsReceiptNoteUpdateDto dto)
    {
        var existing = await repo.GetByIdWithLinesAsync(id);
        if (existing == null)
            return Result<GoodsReceiptNoteReadDto>.Err("GRN not found", "NOT_FOUND");

        var po = await poRepo.GetWithLinesAndGrnsAsync(existing.PurchaseOrderId);
        if (po == null)
            return Result<GoodsReceiptNoteReadDto>.Err("Purchase Order not found", "NOT_FOUND");

        var orderedQty  = po.Lines.Sum(l => l.Quantity);

        // 🚨 exclude this GRN itself from "already received"
        var alreadyReceived = po.GoodsReceiptNotes
            .Where(x => x.Status == GoodsReceiptNoteStatus.Received && x.Id != existing.Id)
            .SelectMany(x => x.Lines)
            .Sum(l => l.Quantity);

        var newQty = dto.Lines.Sum(l => l.Quantity);

        if (alreadyReceived + newQty > orderedQty)
            return Result<GoodsReceiptNoteReadDto>.Err("Cannot receive more than ordered.", "VALIDATION_ERROR");

        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            existing.ReceiptDate = dto.ReceiptDate;

            existing.Lines.Clear();
            foreach (var l in dto.Lines)
            {
                existing.Lines.Add(new GoodsReceiptNoteLine
                {
                    ProductId = l.ProductId,
                    UomId = l.UomId,
                    Quantity = l.Quantity
                });
            }
            
            if (dto.Status == GoodsReceiptNoteStatus.Received && 
                (string.IsNullOrEmpty(existing.GRNNumber) || existing.GRNNumber.StartsWith("Draft GRN")))
            {
                // Generate auto number
                var nextNumber = await repo.GetNextSequenceValueAsync("grn_number_seq", tx);
                existing.GRNNumber = $"GRN/{dto.ReceiptDate.Year}/{nextNumber:000000}";
            }

            if (dto.Status == GoodsReceiptNoteStatus.Received && existing.Status != GoodsReceiptNoteStatus.Received)
            {
                await inventoryService.ApplyGoodsReceiptAsync(existing.Id);
            }
            else if (dto.Status == GoodsReceiptNoteStatus.Cancelled && existing.Status == GoodsReceiptNoteStatus.Received)
            {
                await inventoryService.RevertGoodsReceiptAsync(existing.Id);
            }

            existing.Status = dto.Status;
            await repo.UpdateAsync(existing);
            await tx.CommitAsync();
            return Result<GoodsReceiptNoteReadDto>.Ok(ToReadDto(existing));
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<GoodsReceiptNoteReadDto>.Err($"Update failed: {ex.Message}", "DB_ERROR");
        }
    }


    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        var dn = await repo.GetByIdAsync(id);
        if (dn == null)
            return Result<bool>.Err("GRN not found.", "NOT_FOUND");

        if (dn.Status != GoodsReceiptNoteStatus.Draft)
            return Result<bool>.Err("Only Draft GRN can be deleted.", "VALIDATION_ERROR");

        await repo.DeleteAsync(dn);
        return Result<bool>.Ok(true);
    }

    private static GoodsReceiptNoteListDto ToListDto(GoodsReceiptNote dn) => new GoodsReceiptNoteListDto(
        dn.Id,
        dn.GRNNumber,
        dn.PurchaseOrderId,
        dn.PurchaseOrder?.OrderNumber ?? string.Empty,
        dn.PurchaseOrder?.VendorId ?? Guid.Empty,
        dn.PurchaseOrder?.Vendor?.Name  ?? string.Empty,
        dn.ReceiptDate,
        dn.Status
    );

    private static GoodsReceiptNoteReadDto ToReadDto(GoodsReceiptNote dn) => new GoodsReceiptNoteReadDto(
        dn.Id,
        dn.GRNNumber,
        dn.PurchaseOrderId,
        dn.PurchaseOrder?.OrderNumber ?? string.Empty,
        dn.PurchaseOrder?.VendorId ?? Guid.Empty,
        dn.PurchaseOrder?.Vendor?.Name  ?? string.Empty,
        dn.ReceiptDate,
        dn.Status,
        dn.Lines.Select(line => new GoodsReceiptNoteLineDto(
            line.ProductId,
            line.Product?.Name ?? string.Empty,
            line.UomId,
            line.Uom?.Abbreviation ?? string.Empty,
            line.Quantity
        )).ToList()
    );
}