using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Common.Services;

public interface ISupplierService
{
    Task<Result<PagedResult<Supplier>>> GetPagedAsync(int page, int pageSize, IDictionary<string, string?>? filters, string? sort);
    Task<Result<IReadOnlyList<Supplier>>> GetAllAsync();
    Task<Result<Supplier>> GetByIdAsync(Guid id);
    Task<Result<Supplier>> CreateAsync(SupplierCreateDto dto);
    Task<Result<Supplier>> UpdateAsync(Guid id, SupplierUpdateDto dto);
    Task<Result<bool>> DeleteAsync(Guid id);
}
