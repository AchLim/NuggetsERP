using Nuggets.Application.Common;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Application.Common.Services;
using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Services;

public sealed class SupplierService(ISupplierRepository repo) : ISupplierService
{
    public async Task<Result<PagedResult<Supplier>>> GetPagedAsync(int page, int pageSize, IDictionary<string, string?>? filters, string? sort)
    {
        var (items, totalCount) = await repo.GetPagedAsync(page, pageSize, filters, sort);
        var result = new PagedResult<Supplier>(items, totalCount, page, pageSize);
        return Result<PagedResult<Supplier>>.Ok(result);
    }

    public async Task<Result<Supplier>> CreateAsync(SupplierCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return Result<Supplier>.Err("Full name is required!");

        var customer = new Supplier
        {
            Name = dto.Name,
            Email = dto.Email,
            Address = dto.Address,
            Phone = dto.Phone,
        };

        await repo.AddAsync(customer);
        return Result<Supplier>.Ok(customer);
    }

    public async Task<Result<IReadOnlyList<Supplier>>> GetAllAsync()
    {
        var list = await repo.GetAllAsync();
        return Result<IReadOnlyList<Supplier>>.Ok(list);
    }

    public async Task<Result<Supplier>> GetByIdAsync(Guid id)
    {
        var customer = await repo.GetByIdAsync(id);
        return customer is not null
            ? Result<Supplier>.Ok(customer)
            : Result<Supplier>.Err("Supplier not found!");
    }

    public async Task<Result<Supplier>> UpdateAsync(Guid id, SupplierUpdateDto dto)
    {
        var existing = await repo.GetByIdAsync(id);

        if (existing is null)
            return Result<Supplier>.Err("Supplier not found!");

        existing.Name = dto.Name;
        existing.Email = dto.Email;
        existing.Address = dto.Address;
        existing.Phone = dto.Phone;

        await repo.UpdateAsync(existing);
        return Result<Supplier>.Ok(existing);
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        var existing = await repo.GetByIdAsync(id);
        if (existing is null)
            return Result<bool>.Err("Supplier not found!");

        await repo.DeleteAsync(existing);
        return Result<bool>.Ok(true);
    }
}
