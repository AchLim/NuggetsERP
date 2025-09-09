using Nuggets.Application.Common;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Application.Common.Services;
using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Services;

public class SaleService(ISaleRepository repo) : ISaleService
{
    public async Task<Result<PagedResult<Sale>>> GetPagedAsync(int page, int pageSize, IDictionary<string, string?>? filters, string? sort)
    {
        var (items, totalCount) = await repo.GetPagedAsync(page, pageSize, filters, sort);
        var result = new PagedResult<Sale>(items, totalCount, page, pageSize);
        return Result<PagedResult<Sale>>.Ok(result);
    }
    
    public async Task<Result<IReadOnlyList<Sale>>> GetAllAsync()
    {
        var list = await repo.GetAllAsync();
        return Result<IReadOnlyList<Sale>>.Ok(list);
    }

    public async Task<Result<Sale>> GetByIdAsync(Guid id)
    {
        var entity = await repo.GetByIdAsync(id);
        return entity is not null ? Result<Sale>.Ok(entity) : Result<Sale>.Err("Sale not found!");
    }

    public async Task<Result<Sale>> CreateAsync(SaleCreateDto dto)
    {
        if (dto.Quantity <= 0) return Result<Sale>.Err("Quantity must be greater than zero.");
        if (dto.PricePerUnit <= 0) return Result<Sale>.Err("Price must be greater than zero.");

        var entity = new Sale
        {
            ProductId = dto.ProductId,
            Quantity = dto.Quantity,
            PricePerUnit = dto.PricePerUnit,
            SaleDate = dto.SaleDate
        };

        await repo.AddAsync(entity);
        return Result<Sale>.Ok(entity);
    }

    public async Task<Result<Sale>> UpdateAsync(Guid id, SaleUpdateDto dto)
    {
        var existing = await repo.GetByIdAsync(id);
        if (existing is null) return Result<Sale>.Err("Sale not found!");

        existing.Quantity = dto.Quantity;
        existing.PricePerUnit = dto.PricePerUnit;
        existing.SaleDate = dto.SaleDate;

        await repo.UpdateAsync(existing);
        return Result<Sale>.Ok(existing);
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        var existing = await repo.GetByIdAsync(id);
        if (existing is null) return Result<bool>.Err("Sale not found!");

        await repo.DeleteAsync(existing);
        return Result<bool>.Ok(true);
    }

    public async Task<decimal> TotalRevenueAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var sales = await repo.FindAsync(s => s.SaleDate >= from && s.SaleDate <= to, ct);
        return sales.Sum(s => (decimal)s.Quantity * s.PricePerUnit);
    }
}