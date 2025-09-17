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
    IUomService uomService)
    : IGoodsReceiptNoteService
{
    public async Task<Result<PagedResult<GoodsReceiptNoteListDto>>> GetPagedAsync(int page, int pageSize)
    {
        var (items, totalCount) = await repo.GetPagedAsync(page, pageSize);
        var list = items.Select(ToListDto).ToList();

        return Result<PagedResult<GoodsReceiptNoteListDto>>.Ok(
            new PagedResult<GoodsReceiptNoteListDto>(list, totalCount, page, pageSize));
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
        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing is null)
                return Result<GoodsReceiptNoteReadDto>.Err("GRN not found", "NOT_FOUND");

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
                var nextNumber = await repo.GetNextSequenceValueAsync("grn_number_seq");
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