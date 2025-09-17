using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Common.Services;

public interface IProductService
{
    Task<Result<PagedResult<ProductListDto>>> GetPagedAsync(int page, int pageSize);
    Task<Result<IReadOnlyList<ProductListDto>>> GetAllAsync();
    Task<Result<ProductReadDto>> GetByIdAsync(Guid id);
    Task<Result<ProductReadDto>> CreateAsync(ProductCreateDto dto);
    Task<Result<ProductReadDto>> UpdateAsync(Guid id, ProductUpdateDto dto);
    Task<Result<bool>> DeleteAsync(Guid id);
}
