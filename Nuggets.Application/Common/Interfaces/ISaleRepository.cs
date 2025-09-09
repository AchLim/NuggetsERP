using Nuggets.Domain.Entities;

namespace Nuggets.Application.Common.Interfaces;

public interface ISaleRepository : IGenericRepository<Sale>
{
    Task<decimal> GetTotalRevenueAsync(DateTime from, DateTime to);
}