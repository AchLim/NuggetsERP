using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Common.Services;

public interface IFoodMaterialService
{
    Task<Result<PagedResult<FoodMaterial>>> GetPagedAsync(int page, int pageSize);
    Task<Result<IReadOnlyList<FoodMaterial>>> GetAllAsync();
    Task<Result<FoodMaterial>> GetByIdAsync(Guid id);
    Task<Result<FoodMaterial>> CreateAsync(FoodMaterialCreateDto dto);
    Task<Result<FoodMaterial>> UpdateAsync(Guid id, FoodMaterialUpdateDto dto);
    Task<Result<bool>> DeleteAsync(Guid id);
}