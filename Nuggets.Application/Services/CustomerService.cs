using Nuggets.Application.Common;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Application.Common.Services;
using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Services;

public sealed class CustomerService(ICustomerRepository repo) : ICustomerService
{
    public async Task<Result<PagedResult<Customer>>> GetPagedAsync(int page, int pageSize)
    {
        var (items, totalCount) = await repo.GetPagedAsync(page, pageSize);
        var result = new PagedResult<Customer>(items, totalCount, page, pageSize);
        return Result<PagedResult<Customer>>.Ok(result);
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
            : Result<Customer>.Err("Customer not found!", "NOT_FOUND");
    }
    
    public async Task<Result<Customer>> CreateAsync(CustomerCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return Result<Customer>.Err("Full name is required!", "VALIDATION_ERROR");

        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var customer = new Customer
            {
                Name = dto.Name,
                Email = dto.Email,
                Address = dto.Address,
                Phone = dto.Phone,
            };

            await repo.AddAsync(customer);

            await tx.CommitAsync();
            return Result<Customer>.Ok(customer);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<Customer>.Err($"Failed to create customer: {ex.Message}", "DB_ERROR");
        }
    }

    public async Task<Result<Customer>> UpdateAsync(Guid id, CustomerUpdateDto dto)
    {
        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing is null)
                return Result<Customer>.Err("Customer not found", "NOT_FOUND");

            existing.Name = dto.Name;
            existing.Email = dto.Email;
            existing.Address = dto.Address;
            existing.Phone = dto.Phone;

            await repo.UpdateAsync(existing);

            await tx.CommitAsync();
            return Result<Customer>.Ok(existing);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<Customer>.Err($"Failed to update customer: {ex.Message}", "DB_ERROR");
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing is null)
                return Result<bool>.Err("Customer not found", "NOT_FOUND");

            await repo.DeleteAsync(existing);

            await tx.CommitAsync();
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<bool>.Err($"Failed to delete customer: {ex.Message}", "DB_ERROR");
        }
    }
}
