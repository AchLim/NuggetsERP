using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Common.Services;

public interface IProductService
{
    Task<Result<PagedResult<Product>>> GetPagedAsync(int page, int pageSize, IDictionary<string, string?>? filters, string? sort);
    Task<Result<IReadOnlyList<Product>>> GetAllAsync();
    Task<Result<Product>> GetByIdAsync(Guid id);
    Task<Result<Product>> CreateAsync(ProductCreateDto dto);
    Task<Result<Product>> UpdateAsync(Guid id, ProductUpdateDto dto);
    Task<Result<bool>> DeleteAsync(Guid id);
}
