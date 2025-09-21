using Nuggets.Application.Common;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Application.Common.Services;
using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Services;

public sealed class SalesOrderService(ISalesOrderRepository repo) : ISalesOrderService
{
    public async Task<Result<PagedResult<SalesOrderListDto>>> GetPagedAsync(int page, int pageSize)
    {
        var (items, total) = await repo.GetPagedAsync(page, pageSize);
        var list = items.Select(ToListDto).ToList();
        return Result<PagedResult<SalesOrderListDto>>.Ok(new PagedResult<SalesOrderListDto>(list, total, page, pageSize));
    }

    public async Task<Result<IReadOnlyList<SalesOrderListDto>>> GetAllAsync()
    {
        var items = await repo.GetAllAsync();
        return Result<IReadOnlyList<SalesOrderListDto>>.Ok(items.Select(ToListDto).ToList());
    }

    public async Task<Result<SalesOrderReadDto>> GetByIdAsync(Guid id)
    {
        var ent = await repo.GetByIdAsync(id);
        return ent is not null ? Result<SalesOrderReadDto>.Ok(ToReadDto(ent)) : Result<SalesOrderReadDto>.Err("Sales order not found", "NOT_FOUND");
    }

    public async Task<Result<SalesOrderReadDto>> CreateAsync(SalesOrderCreateDto dto)
    {
        if (dto.CustomerId == Guid.Empty) return Result<SalesOrderReadDto>.Err("Customer required", "VALIDATION_ERROR");
        if (dto.Lines.Count == 0) return Result<SalesOrderReadDto>.Err("At least one line required", "VALIDATION_ERROR");

        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var draftLabel = $"Draft SO *{DateTime.UtcNow:yyyyMMddHHmmss}";
            
            var ent = new SalesOrder
            {
                CustomerId = dto.CustomerId,
                OrderNumber = draftLabel,
                OrderDate = dto.OrderDate,
                Status = SalesOrderStatus.Draft,
                Lines = dto.Lines.Select(l => new SalesOrderLine
                {
                    ProductId = l.ProductId,
                    UomId = l.UomId,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice,
                    DiscountPercent = l.DiscountPercent
                }).ToList()
            };

            await repo.AddAsync(ent);
            await tx.CommitAsync();
            return Result<SalesOrderReadDto>.Ok(ToReadDto(ent));
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<SalesOrderReadDto>.Err($"Failed to create sales order: {ex.Message}", "DB_ERROR");
        }
    }

    public async Task<Result<SalesOrderReadDto>> UpdateAsync(Guid id, SalesOrderUpdateDto dto)
    {
        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing is null) return Result<SalesOrderReadDto>.Err("Sales order not found", "NOT_FOUND");
            
            if (dto.Status == SalesOrderStatus.Confirmed && 
                (string.IsNullOrEmpty(existing.OrderNumber) || existing.OrderNumber.StartsWith("Draft SO")))
            {
                // Generate auto number
                var nextNumber = await repo.GetNextSequenceValueAsync("sales_order_number_seq");
                existing.OrderNumber = $"SO/{dto.OrderDate.Year}/{nextNumber:000000}";
            }
            
            existing.CustomerId = dto.CustomerId;
            existing.OrderDate = dto.OrderDate;
            existing.Status = dto.Status;

            existing.Lines.Clear();
            foreach (var l in dto.Lines)
            {
                existing.Lines.Add(new SalesOrderLine
                {
                    ProductId = l.ProductId,
                    UomId = l.UomId,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice,
                    DiscountPercent = l.DiscountPercent
                });
            }

            await repo.UpdateAsync(existing);
            await tx.CommitAsync();
            return Result<SalesOrderReadDto>.Ok(ToReadDto(existing));
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<SalesOrderReadDto>.Err($"Failed to update: {ex.Message}", "DB_ERROR");
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing is null) return Result<bool>.Err("Sales order not found", "NOT_FOUND");
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

    private static SalesOrderListDto ToListDto(SalesOrder o) =>
        new(o.Id, o.CustomerId, o.Customer?.Name, o.OrderNumber, o.OrderDate, o.Status,
            o.Lines.Sum(l => l.LineTotal));

    private static SalesOrderReadDto ToReadDto(SalesOrder so)
    {
        var orderedQty = so.Lines.Sum(l => l.Quantity);

        var deliveredQty = so.DeliveryNotes
            .Where(dn => dn.Status == DeliveryNoteStatus.Delivered)
            .SelectMany(dn => dn.Lines)
            .Sum(l => l.Quantity);

        var invoicedQty = so.CustomerInvoices
            .Where(ci => ci.Status is CustomerInvoiceStatus.Posted or CustomerInvoiceStatus.Paid)
            .SelectMany(ci => ci.Lines)
            .Sum(l => l.Quantity);

        return new SalesOrderReadDto(
            so.Id,
            so.CustomerId,
            so.Customer?.Name,
            so.OrderNumber,
            so.OrderDate,
            so.Status,
            so.Lines.Select(l => new SalesOrderLineReadDto(
                l.Id, l.ProductId, l.Product?.Name, l.UomId, l.Quantity, l.UnitPrice, l.DiscountPercent, l.LineTotal
            )).ToList(),
            orderedQty,
            deliveredQty,
            invoicedQty
        );
    }
}
