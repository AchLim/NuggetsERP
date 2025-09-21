using Nuggets.Application.Common;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Application.Common.Services;
using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Services;

public sealed class DeliveryNoteService(
    IDeliveryNoteRepository repo,
    IProductRepository productRepo,
    ISalesOrderRepository soRepo,
    IInventoryService inventoryService,
    IUomService uomService)
    : IDeliveryNoteService
{
    public async Task<Result<PagedResult<DeliveryNoteListDto>>> GetPagedAsync(int page, int pageSize, Guid? salesOrderId = null)
    {
        var result = salesOrderId.HasValue
            ? await repo.GetPagedAsync(page, pageSize, repo.Query().Where(dn => dn.SalesOrderId == salesOrderId.Value))
            : await repo.GetPagedAsync(page, pageSize);

        var list = result.Items.Select(ToListDto).ToList();
        return Result<PagedResult<DeliveryNoteListDto>>.Ok(
            new PagedResult<DeliveryNoteListDto>(list, result.TotalCount, page, pageSize));
    }

    public async Task<Result<DeliveryNoteReadDto>> GetByIdAsync(Guid id)
    {
        var dn = await repo.GetByIdWithLinesAsync(id);
        if (dn == null)
            return Result<DeliveryNoteReadDto>.Err("Delivery Note not found.", "NOT_FOUND");

        return Result<DeliveryNoteReadDto>.Ok(ToReadDto(dn));
    }

    public async Task<Result<DeliveryNoteReadDto>> CreateAsync(DeliveryNoteCreateDto dto)
    {
        var so = await soRepo.GetWithLinesAndDnsAsync(dto.SalesOrderId);
        if (so == null)
            return Result<DeliveryNoteReadDto>.Err("Sales Order not found", "NOT_FOUND");

        var orderedQty = so.Lines.Sum(l => l.Quantity);

        var deliveredQty = so.DeliveryNotes
            .Where(dn => dn.Status is DeliveryNoteStatus.Delivered)
            .SelectMany(dn => dn.Lines)
            .Sum(l => l.Quantity);

        var newQty = dto.Lines.Sum(l => l.Quantity);

        if (deliveredQty + newQty > orderedQty)
            return Result<DeliveryNoteReadDto>.Err("Cannot deliver more than ordered.", "VALIDATION_ERROR");
        
        var draftLabel = $"Draft DN *{DateTime.UtcNow:yyyyMMddHHmmss}";

        var dn = new DeliveryNote
        {
            DeliveryNumber = draftLabel,
            SalesOrderId = dto.SalesOrderId,
            DeliveryDate = dto.DeliveryDate,
            Status = DeliveryNoteStatus.Draft,
            Lines = dto.Lines.Select(lineDto => new DeliveryNoteLine
            {
                ProductId = lineDto.ProductId,
                UomId = lineDto.UomId,
                Quantity = lineDto.Quantity
            }).ToList()
        };

        await repo.AddAsync(dn);
        return await GetByIdAsync(dn.Id);
    }

    public async Task<Result<DeliveryNoteReadDto>> UpdateAsync(Guid id, DeliveryNoteUpdateDto dto)
    {
        var existing = await repo.GetByIdWithLinesAsync(id);
        if (existing == null)
            return Result<DeliveryNoteReadDto>.Err("Delivery Note not found.", "NOT_FOUND");

        var so = await soRepo.GetWithLinesAndDnsAsync(existing.SalesOrderId);
        if (so == null)
            return Result<DeliveryNoteReadDto>.Err("Sales Order not found.", "NOT_FOUND");

        var orderedQty = so.Lines.Sum(l => l.Quantity);

        var deliveredQty = so.DeliveryNotes
            .Where(dn => dn.Status == DeliveryNoteStatus.Delivered && dn.Id != existing.Id)
            .SelectMany(dn => dn.Lines)
            .Sum(l => l.Quantity);

        var newQty = dto.Lines.Sum(l => l.Quantity);

        if (deliveredQty + newQty > orderedQty)
            return Result<DeliveryNoteReadDto>.Err("Cannot deliver more than ordered.", "VALIDATION_ERROR");

        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            existing.DeliveryDate = dto.DeliveryDate;

            existing.Lines.Clear();
            foreach (var l in dto.Lines)
            {
                existing.Lines.Add(new DeliveryNoteLine
                {
                    ProductId = l.ProductId,
                    UomId = l.UomId,
                    Quantity = l.Quantity
                });
            }
            
            if (dto.Status == DeliveryNoteStatus.Delivered && 
                (string.IsNullOrEmpty(existing.DeliveryNumber) || existing.DeliveryNumber.StartsWith("Draft DN")))
            {
                // Generate auto number
                var nextNumber = await repo.GetNextSequenceValueAsync("dn_number_seq");
                existing.DeliveryNumber = $"DN/{dto.DeliveryDate.Year}/{nextNumber:000000}";
            }

            if (dto.Status == DeliveryNoteStatus.Delivered && existing.Status != DeliveryNoteStatus.Delivered)
            {
                await inventoryService.ApplyDeliveryNoteAsync(existing.Id);
            }
            else if (dto.Status == DeliveryNoteStatus.Cancelled && existing.Status == DeliveryNoteStatus.Delivered)
            {
                await inventoryService.RevertDeliveryNoteAsync(existing.Id);
            }

            existing.Status = dto.Status;
            await repo.UpdateAsync(existing);
            await tx.CommitAsync();
            return Result<DeliveryNoteReadDto>.Ok(ToReadDto(existing));
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<DeliveryNoteReadDto>.Err($"Update failed: {ex.Message}", "DB_ERROR");
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        var dn = await repo.GetByIdAsync(id);
        if (dn == null)
            return Result<bool>.Err("Delivery Note not found.", "NOT_FOUND");

        if (dn.Status != DeliveryNoteStatus.Draft)
            return Result<bool>.Err("Only Draft delivery notes can be deleted.", "VALIDATION_ERROR");

        await repo.DeleteAsync(dn);
        return Result<bool>.Ok(true);
    }

    private static DeliveryNoteListDto ToListDto(DeliveryNote dn) => new DeliveryNoteListDto(
        dn.Id,
        dn.DeliveryNumber,
        dn.SalesOrderId,
        dn.SalesOrder?.OrderNumber ?? string.Empty,
        dn.SalesOrder?.CustomerId ?? Guid.Empty,
        dn.SalesOrder?.Customer?.Name  ?? string.Empty,
        dn.DeliveryDate,
        dn.Status
    );

    private static DeliveryNoteReadDto ToReadDto(DeliveryNote dn) => new DeliveryNoteReadDto(
        dn.Id,
        dn.DeliveryNumber,
        dn.SalesOrderId,
        dn.SalesOrder?.OrderNumber ?? string.Empty,
        dn.SalesOrder?.CustomerId ?? Guid.Empty,
        dn.SalesOrder?.Customer?.Name  ?? string.Empty,
        dn.DeliveryDate,
        dn.Status,
        dn.Lines.Select(line => new DeliveryNoteLineDto(
            line.ProductId,
            line.Product?.Name ?? string.Empty,
            line.UomId,
            line.Uom?.Abbreviation ?? string.Empty,
            line.Quantity
        )).ToList()
    );
}