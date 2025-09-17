using Nuggets.Application.Common;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Application.Common.Services;
using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Services;

public class ExpenseService(IExpenseRepository repo) : IExpenseService
{
    public async Task<Result<PagedResult<Expense>>> GetPagedAsync(int page, int pageSize)
    {
        var (items, totalCount) = await repo.GetPagedAsync(page, pageSize);
        return Result<PagedResult<Expense>>.Ok(new PagedResult<Expense>(items, totalCount, page, pageSize));
    }

    public async Task<Result<IReadOnlyList<Expense>>> GetAllAsync()
    {
        var list = await repo.GetAllAsync();
        return Result<IReadOnlyList<Expense>>.Ok(list);
    }

    public async Task<Result<Expense>> GetByIdAsync(Guid id)
    {
        var entity = await repo.GetByIdAsync(id);
        return entity is not null
            ? Result<Expense>.Ok(entity)
            : Result<Expense>.Err("Expense not found", "NOT_FOUND");
    }

    public async Task<Result<Expense>> CreateAsync(ExpenseCreateDto dto)
    {
        if (dto.Amount <= 0)
            return Result<Expense>.Err("Amount must be greater than zero", "VALIDATION_ERROR");

        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var entity = new Expense
            {
                Description = dto.Description,
                Category = dto.Category,
                Amount = dto.Amount,
                ExpenseDate = dto.ExpenseDate
            };
            await repo.AddAsync(entity);
            await tx.CommitAsync();
            return Result<Expense>.Ok(entity);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<Expense>.Err($"Failed to create expense: {ex.Message}", "DB_ERROR");
        }
    }

    public async Task<Result<Expense>> UpdateAsync(Guid id, ExpenseUpdateDto dto)
    {
        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing is null)
                return Result<Expense>.Err("Expense not found", "NOT_FOUND");

            existing.Description = dto.Description;
            existing.Category = dto.Category;
            existing.Amount = dto.Amount;
            existing.ExpenseDate = dto.ExpenseDate;
            await repo.UpdateAsync(existing);

            await tx.CommitAsync();
            return Result<Expense>.Ok(existing);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<Expense>.Err($"Failed to update expense: {ex.Message}", "DB_ERROR");
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing is null)
                return Result<bool>.Err("Expense not found", "NOT_FOUND");

            await repo.DeleteAsync(existing);
            await tx.CommitAsync();
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<bool>.Err($"Failed to delete expense: {ex.Message}", "DB_ERROR");
        }
    }
}