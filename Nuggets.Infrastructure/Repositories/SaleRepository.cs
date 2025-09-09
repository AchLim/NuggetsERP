using Microsoft.EntityFrameworkCore;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Domain.Entities;
using Nuggets.Infrastructure.Persistence;

namespace Nuggets.Infrastructure.Repositories;

public sealed class SaleRepository(NuggetsDbContext context) : GenericRepository<Sale>(context), ISaleRepository
{
    private readonly NuggetsDbContext _context = context;

    public async Task<decimal> GetTotalRevenueAsync(DateTime from, DateTime to)
    {
        return await _context.Sales
            .Where(s => s.SaleDate >= from && s.SaleDate <= to)
            .SumAsync(s => (decimal)s.Quantity * s.PricePerUnit);
    }
}