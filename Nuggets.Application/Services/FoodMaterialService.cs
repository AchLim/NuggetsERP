using Nuggets.Application.Common;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Application.Common.Services;
using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Services;

public class FoodMaterialService(IFoodMaterialRepository repo) : IFoodMaterialService
{
    public async Task<Result<PagedResult<FoodMaterial>>> GetPagedAsync(int page, int pageSize, IDictionary<string, string?>? filters, string? sort)
    {
        var (items, totalCount) = await repo.GetPagedAsync(page, pageSize, filters, sort);
        var result = new PagedResult<FoodMaterial>(items, totalCount, page, pageSize);
        return Result<PagedResult<FoodMaterial>>.Ok(result);
    }
    public async Task<Result<IReadOnlyList<FoodMaterial>>> GetAllAsync()
    {
        var list = await repo.GetAllAsync();
        return Result<IReadOnlyList<FoodMaterial>>.Ok(list);
    }

    public async Task<Result<FoodMaterial>> GetByIdAsync(Guid id)
    {
        var entity = await repo.GetByIdAsync(id);
        return entity is not null ? Result<FoodMaterial>.Ok(entity) : Result<FoodMaterial>.Err("Material not found!");
    }

    public async Task<Result<FoodMaterial>> CreateAsync(FoodMaterialCreateDto dto)
    {
        var entity = new FoodMaterial { Name = dto.Name, Active = true, PricePerUnit = dto.PricePerUnit, UomId = dto.UomId };
        await repo.AddAsync(entity);
        return Result<FoodMaterial>.Ok(entity);
    }

    public async Task<Result<FoodMaterial>> UpdateAsync(Guid id, FoodMaterialUpdateDto dto)
    {
        var existing = await repo.GetByIdAsync(id);
        if (existing is null) return Result<FoodMaterial>.Err("Not found!");

        existing.Name = dto.Name;
        existing.PricePerUnit = dto.PricePerUnit;
        existing.UomId = dto.UomId;

        await repo.UpdateAsync(existing);
        return Result<FoodMaterial>.Ok(existing);
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        var existing = await repo.GetByIdAsync(id);
        if (existing is null) return Result<bool>.Err("Not found!");

        await repo.DeleteAsync(existing);
        return Result<bool>.Ok(true);
    }
}