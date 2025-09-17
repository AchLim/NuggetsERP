using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Common.Services;

public interface IProductCategoryService
{
    Task<Result<PagedResult<ProductCategory>>> GetPagedAsync(int page, int pageSize);
    Task<Result<IReadOnlyList<ProductCategory>>> GetAllAsync();
    Task<Result<ProductCategory>> GetByIdAsync(Guid id);
    Task<Result<ProductCategory>> CreateAsync(ProductCategoryCreateDto dto);
    Task<Result<ProductCategory>> UpdateAsync(Guid id, ProductCategoryUpdateDto dto);
    Task<Result<bool>> DeleteAsync(Guid id);
}
