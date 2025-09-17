using Nuggets.Application.Common.Interfaces;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Services;

public interface IReportingService
{
    // Task<decimal> CalculateNetProfitAsync(DateTime from, DateTime to);
}

// public class ReportingService(ISaleRepository saleRepo, IGenericRepository<Expense> expenseRepo)
//     : IReportingService
// {
//     // public async Task<decimal> CalculateNetProfitAsync(DateTime from, DateTime to)
//     // {
//     //     var revenue = await saleRepo.GetTotalRevenueAsync(from, to);
//     //
//     //     var expenses = (await expenseRepo.FindAsync(x => x.ExpenseDate >= from && x.ExpenseDate <= to))
//     //         .Sum(e => e.Amount);
//     //
//     //     return revenue - expenses;
//     // }
// }