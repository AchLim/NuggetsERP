using Nuggets.Application.Common;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Application.Common.Services;
using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Services;

public class ExpenseService(IExpenseRepository repo) : IExpenseService
{
    public async Task<Result<PagedResult<Expense>>> GetPagedAsync(int page, int pageSize, IDictionary<string, string?>? filters, string? sort)
    {
        var (items, totalCount) = await repo.GetPagedAsync(page, pageSize, filters, sort);
        var result = new PagedResult<Expense>(items, totalCount, page, pageSize);
        return Result<PagedResult<Expense>>.Ok(result);
    }

    public async Task<Result<IReadOnlyList<Expense>>> GetAllAsync()
    {
        var list = await repo.GetAllAsync();
        return Result<IReadOnlyList<Expense>>.Ok(list);
    }

    public async Task<Result<Expense>> GetByIdAsync(Guid id)
    {
        var entity = await repo.GetByIdAsync(id);
        return entity is not null ? Result<Expense>.Ok(entity) : Result<Expense>.Err("Expense not found!");
    }

    public async Task<Result<Expense>> CreateAsync(ExpenseCreateDto dto)
    {
        if (dto.Amount <= 0) return Result<Expense>.Err("Amount must be greater than zero.");

        var entity = new Expense
        {
            Description = dto.Description,
            Category = dto.Category,
            Amount = dto.Amount,
            ExpenseDate = dto.ExpenseDate
        };

        await repo.AddAsync(entity);
        return Result<Expense>.Ok(entity);
    }

    public async Task<Result<Expense>> UpdateAsync(Guid id, ExpenseUpdateDto dto)
    {
        var existing = await repo.GetByIdAsync(id);
        if (existing is null) return Result<Expense>.Err("Expense not found!");

        existing.Description = dto.Description;
        existing.Category = dto.Category;
        existing.Amount = dto.Amount;
        existing.ExpenseDate = dto.ExpenseDate;

        await repo.UpdateAsync(existing);
        return Result<Expense>.Ok(existing);
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        var existing = await repo.GetByIdAsync(id);
        if (existing is null) return Result<bool>.Err("Expense not found!");

        await repo.DeleteAsync(existing);
        return Result<bool>.Ok(true);
    }
}