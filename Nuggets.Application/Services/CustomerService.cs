using Nuggets.Application.Common;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Application.Common.Services;
using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Services;

public sealed class CustomerService(ICustomerRepository repo) : ICustomerService
{
    public async Task<Result<PagedResult<Customer>>> GetPagedAsync(int page, int pageSize, IDictionary<string, string?>? filters, string? sort)
    {
        var (items, totalCount) = await repo.GetPagedAsync(page, pageSize, filters, sort);
        var result = new PagedResult<Customer>(items, totalCount, page, pageSize);
        return Result<PagedResult<Customer>>.Ok(result);
    }
    
    public async Task<Result<Customer>> CreateAsync(CustomerCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return Result<Customer>.Err("Full name is required!");

        var customer = new Customer
        {
            Name = dto.Name,
            Email = dto.Email,
            Address = dto.Address,
            Phone = dto.Phone,
        };

        await repo.AddAsync(customer);
        return Result<Customer>.Ok(customer);
    }

    public async Task<Result<IReadOnlyList<Customer>>> GetAllAsync()
    {
        var list = await repo.GetAllAsync();
        return Result<IReadOnlyList<Customer>>.Ok(list);
    }

    public async Task<Result<Customer>> GetByIdAsync(Guid id)
    {
        var customer = await repo.GetByIdAsync(id);
        return customer is not null
            ? Result<Customer>.Ok(customer)
            : Result<Customer>.Err("Customer not found!");
    }

    public async Task<Result<Customer>> UpdateAsync(Guid id, CustomerUpdateDto dto)
    {
        var existing = await repo.GetByIdAsync(id);

        if (existing is null)
            return Result<Customer>.Err("Customer not found!");

        existing.Name = dto.Name;
        existing.Email = dto.Email;
        existing.Address = dto.Address;
        existing.Phone = dto.Phone;

        await repo.UpdateAsync(existing);
        return Result<Customer>.Ok(existing);
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        var existing = await repo.GetByIdAsync(id);
        if (existing is null)
            return Result<bool>.Err("Customer not found!");

        await repo.DeleteAsync(existing);
        return Result<bool>.Ok(true);
    }
}
