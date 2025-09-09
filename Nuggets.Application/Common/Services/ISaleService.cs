using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Common.Services;

public interface ISaleService
{
    Task<Result<PagedResult<Sale>>> GetPagedAsync(int page, int pageSize, IDictionary<string, string?>? filters, string? sort);
    Task<Result<IReadOnlyList<Sale>>> GetAllAsync();
    Task<Result<Sale>> GetByIdAsync(Guid id);
    Task<Result<Sale>> CreateAsync(SaleCreateDto dto);
    Task<Result<Sale>> UpdateAsync(Guid id, SaleUpdateDto dto);
    Task<Result<bool>> DeleteAsync(Guid id);
    Task<decimal> TotalRevenueAsync(DateTime from, DateTime to, CancellationToken ct = default);
}