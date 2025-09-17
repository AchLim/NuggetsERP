using Nuggets.Application.Common;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Application.Common.Services;
using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Services;

public class FoodRecipeService(IFoodRecipeRepository recipeRepo, IFoodMaterialRepository materialRepo) : IFoodRecipeService
{
    public async Task<Result<PagedResult<FoodRecipe>>> GetPagedAsync(
        int page, int pageSize)
    {
        var (items, totalCount) = await recipeRepo.GetPagedAsync(page, pageSize);
        return Result<PagedResult<FoodRecipe>>.Ok(new PagedResult<FoodRecipe>(items, totalCount, page, pageSize));
    }

    public async Task<Result<IReadOnlyList<FoodRecipe>>> GetAllAsync()
    {
        var list = await recipeRepo.GetAllAsync();
        return Result<IReadOnlyList<FoodRecipe>>.Ok(list);
    }

    public async Task<Result<FoodRecipe>> GetByIdAsync(Guid id)
    {
        var entity = await recipeRepo.GetByIdAsync(id);
        return entity is not null
            ? Result<FoodRecipe>.Ok(entity)
            : Result<FoodRecipe>.Err("Recipe not found", "NOT_FOUND");
    }

    public async Task<Result<FoodRecipe>> CreateAsync(FoodRecipeCreateDto dto)
    {
        if (dto.Quantity <= 0)
            return Result<FoodRecipe>.Err("Quantity must be positive", "VALIDATION_ERROR");

        await using var tx = await recipeRepo.BeginTransactionAsync();
        try
        {
            var entity = new FoodRecipe
            {
                ProductId = dto.ProductId,
                FoodMaterialId = dto.FoodMaterialId,
                Quantity = dto.Quantity,
                UomId = dto.UomId
            };

            await recipeRepo.AddAsync(entity);
            await tx.CommitAsync();

            return Result<FoodRecipe>.Ok(entity);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<FoodRecipe>.Err($"Failed to create recipe: {ex.Message}", "DB_ERROR");
        }
    }

    public async Task<Result<FoodRecipe>> UpdateAsync(Guid id, FoodRecipeUpdateDto dto)
    {
        await using var tx = await recipeRepo.BeginTransactionAsync();
        try
        {
            var existing = await recipeRepo.GetByIdAsync(id);
            if (existing is null)
                return Result<FoodRecipe>.Err("Recipe not found", "NOT_FOUND");

            existing.Quantity = dto.Quantity;
            existing.UomId = dto.UomId;
            await recipeRepo.UpdateAsync(existing);
            
            await tx.CommitAsync();
            return Result<FoodRecipe>.Ok(existing);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<FoodRecipe>.Err($"Failed to update recipe: {ex.Message}", "DB_ERROR");
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        await using var tx = await recipeRepo.BeginTransactionAsync();
        try
        {
            var existing = await recipeRepo.GetByIdAsync(id);
            if (existing is null)
                return Result<bool>.Err("Recipe not found", "NOT_FOUND");

            await recipeRepo.DeleteAsync(existing);

            await tx.CommitAsync();
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<bool>.Err($"Failed to delete recipe: {ex.Message}", "DB_ERROR");
        }
    }

    public async Task<decimal> CalculateMaterialCostAsync(Guid productId, CancellationToken ct = default)
    {
        var recipes = await recipeRepo.GetByProductIdAsync(productId, ct);
        return recipes.Sum(r => r.FoodMaterial.UnitPrice * r.Quantity);
    }
}