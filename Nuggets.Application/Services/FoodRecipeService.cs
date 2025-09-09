using Nuggets.Application.Common;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Application.Common.Services;
using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Services;

public class FoodRecipeService(IFoodRecipeRepository recipeRepo, IFoodMaterialRepository materialRepo) : IFoodRecipeService
{
    public async Task<Result<PagedResult<FoodRecipe>>> GetPagedAsync(int page, int pageSize, IDictionary<string, string?>? filters, string? sort)
    {
        var (items, totalCount) = await recipeRepo.GetPagedAsync(page, pageSize, filters, sort);
        var result = new PagedResult<FoodRecipe>(items, totalCount, page, pageSize);
        return Result<PagedResult<FoodRecipe>>.Ok(result);
    }
    
    public async Task<Result<IReadOnlyList<FoodRecipe>>> GetAllAsync()
    {
        var list = await recipeRepo.GetAllAsync();
        return Result<IReadOnlyList<FoodRecipe>>.Ok(list);
    }

    public async Task<Result<FoodRecipe>> GetByIdAsync(Guid id)
    {
        var entity = await recipeRepo.GetByIdAsync(id);
        return entity is not null ? Result<FoodRecipe>.Ok(entity) : Result<FoodRecipe>.Err("Recipe not found!");
    }

    public async Task<Result<FoodRecipe>> CreateAsync(FoodRecipeCreateDto dto)
    {
        if (dto.Quantity <= 0) 
            return Result<FoodRecipe>.Err("Quantity must be positive!");

        var entity = new FoodRecipe
        {
            ProductId = dto.ProductId,
            FoodMaterialId = dto.FoodMaterialId,
            Quantity = dto.Quantity,
            UomId = dto.UomId
        };

        await recipeRepo.AddAsync(entity);
        return Result<FoodRecipe>.Ok(entity);
    }

    public async Task<Result<FoodRecipe>> UpdateAsync(Guid id, FoodRecipeUpdateDto dto)
    {
        var existing = await recipeRepo.GetByIdAsync(id);
        if (existing is null) return Result<FoodRecipe>.Err("Recipe not found!");

        existing.Quantity = dto.Quantity;
        existing.UomId = dto.UomId;

        await recipeRepo.UpdateAsync(existing);
        return Result<FoodRecipe>.Ok(existing);
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        var existing = await recipeRepo.GetByIdAsync(id);
        if (existing is null) return Result<bool>.Err("Recipe not found!");

        await recipeRepo.DeleteAsync(existing);
        return Result<bool>.Ok(true);
    }

    public async Task<decimal> CalculateMaterialCostAsync(Guid productId, CancellationToken ct = default)
    {
        var recipes = await recipeRepo.GetByProductIdAsync(productId, ct);

        return recipes.Sum(r => r.FoodMaterial.PricePerUnit * r.Quantity);
    }
}