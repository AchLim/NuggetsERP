using Nuggets.Application.Common;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Application.Common.Services;
using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Services;

public class FoodMaterialService(IFoodMaterialRepository repo) : IFoodMaterialService
{
    public async Task<Result<PagedResult<FoodMaterial>>> GetPagedAsync(int page, int pageSize)
    {
        var (items, totalCount) = await repo.GetPagedAsync(page, pageSize);
        return Result<PagedResult<FoodMaterial>>.Ok(new PagedResult<FoodMaterial>(items, totalCount, page, pageSize));
    }
    
    public async Task<Result<IReadOnlyList<FoodMaterial>>> GetAllAsync()
    {
        var list = await repo.GetAllAsync();
        return Result<IReadOnlyList<FoodMaterial>>.Ok(list);
    }

    public async Task<Result<FoodMaterial>> GetByIdAsync(Guid id)
    {
        var entity = await repo.GetByIdAsync(id);
        return entity is not null
            ? Result<FoodMaterial>.Ok(entity)
            : Result<FoodMaterial>.Err("Material not found", "NOT_FOUND");
    }

    public async Task<Result<FoodMaterial>> CreateAsync(FoodMaterialCreateDto dto)
    {
        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var entity = new FoodMaterial
            {
                Name = dto.Name,
                Active = true,
                UnitPrice = dto.UnitPrice,
                UomId = dto.UomId
            };
            await repo.AddAsync(entity);
            await tx.CommitAsync();
            return Result<FoodMaterial>.Ok(entity);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<FoodMaterial>.Err($"Failed to create material: {ex.Message}", "DB_ERROR");
        }
    }

    public async Task<Result<FoodMaterial>> UpdateAsync(Guid id, FoodMaterialUpdateDto dto)
    {
        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing is null)
                return Result<FoodMaterial>.Err("Material not found", "NOT_FOUND");

            existing.Name = dto.Name;
            existing.UnitPrice = dto.UnitPrice;
            existing.UomId = dto.UomId;
            await repo.UpdateAsync(existing);

            await tx.CommitAsync();
            return Result<FoodMaterial>.Ok(existing);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<FoodMaterial>.Err($"Failed to update material: {ex.Message}", "DB_ERROR");
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing is null)
                return Result<bool>.Err("Material not found", "NOT_FOUND");

            await repo.DeleteAsync(existing);
            await tx.CommitAsync();
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<bool>.Err($"Failed to delete material: {ex.Message}", "DB_ERROR");
        }
    }
}