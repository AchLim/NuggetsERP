using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Common.Services;

public interface IFoodRecipeService
{
    Task<Result<PagedResult<FoodRecipe>>> GetPagedAsync(int page, int pageSize, IDictionary<string, string?>? filters, string? sort);
    Task<Result<IReadOnlyList<FoodRecipe>>> GetAllAsync();
    Task<Result<FoodRecipe>> GetByIdAsync(Guid id);
    Task<Result<FoodRecipe>> CreateAsync(FoodRecipeCreateDto dto);
    Task<Result<FoodRecipe>> UpdateAsync(Guid id, FoodRecipeUpdateDto dto);
    Task<Result<bool>> DeleteAsync(Guid id);
    
    
    Task<decimal> CalculateMaterialCostAsync(Guid productId, CancellationToken ct = default);
}