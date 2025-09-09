using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Common.Services;

public interface IExpenseService
{
    Task<Result<PagedResult<Expense>>> GetPagedAsync(int page, int pageSize, IDictionary<string, string?>? filters, string? sort);
    Task<Result<IReadOnlyList<Expense>>> GetAllAsync();
    Task<Result<Expense>> GetByIdAsync(Guid id);
    Task<Result<Expense>> CreateAsync(ExpenseCreateDto dto);
    Task<Result<Expense>> UpdateAsync(Guid id, ExpenseUpdateDto dto);
    Task<Result<bool>> DeleteAsync(Guid id);
}