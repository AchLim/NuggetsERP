using Nuggets.Domain.Entities;

namespace Nuggets.Application.Common.Interfaces;

public interface IExpenseRepository : IGenericRepository<Expense>
{
    Task<IReadOnlyList<Expense>> GetByCategoryAsync(ExpenseCategory category, CancellationToken ct = default);
    Task<IReadOnlyList<Expense>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct = default);
}