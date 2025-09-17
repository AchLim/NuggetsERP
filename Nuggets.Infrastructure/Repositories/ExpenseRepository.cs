// using Microsoft.EntityFrameworkCore;
// using Nuggets.Application.Common.Interfaces;
// using Nuggets.Domain.Entities;
// using Nuggets.Infrastructure.Persistence;
//
// namespace Nuggets.Infrastructure.Repositories;
//
// public class ExpenseRepository(NuggetsDbContext db) : GenericRepository<Expense>(db), IExpenseRepository
// {
//     private readonly NuggetsDbContext _db = db;
//
//     public async Task<IReadOnlyList<Expense>> GetByCategoryAsync(ExpenseCategory category, CancellationToken ct = default)
//     {
//         return await _db.Expenses
//             .Where(e => e.Category == category)
//             .OrderBy(e => e.ExpenseDate)
//             .AsNoTracking()
//             .ToListAsync(ct);
//     }
//
//     public async Task<IReadOnlyList<Expense>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct = default)
//     {
//         return await _db.Expenses
//             .Where(e => e.ExpenseDate >= from && e.ExpenseDate <= to)
//             .OrderBy(e => e.ExpenseDate)
//             .AsNoTracking()
//             .ToListAsync(ct);
//     }
// }