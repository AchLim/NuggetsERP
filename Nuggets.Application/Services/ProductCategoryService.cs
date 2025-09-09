using Nuggets.Application.Common;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Application.Common.Services;
using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Services;
public class ProductCategoryService(IProductCategoryRepository repo) : IProductCategoryService
{
    public async Task<Result<PagedResult<ProductCategory>>> GetPagedAsync(int page, int pageSize, IDictionary<string, string?>? filters, string? sort)
    {
        var (items, totalCount) = await repo.GetPagedAsync(page, pageSize, filters, sort);
        var result = new PagedResult<ProductCategory>(items, totalCount, page, pageSize);
        return Result<PagedResult<ProductCategory>>.Ok(result);
    }
    
    public async Task<Result<IReadOnlyList<ProductCategory>>> GetAllAsync()
    {
        var list = await repo.GetAllAsync();
        return Result<IReadOnlyList<ProductCategory>>.Ok(list);
    }

    public async Task<Result<ProductCategory>> GetByIdAsync(Guid id)
    {
        var category = await repo.GetByIdAsync(id);
        return category is not null
            ? Result<ProductCategory>.Ok(category)
            : Result<ProductCategory>.Err("Category not found!");
    }

    public async Task<Result<ProductCategory>> CreateAsync(ProductCategoryCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return Result<ProductCategory>.Err("Category name is required!");

        var entity = new ProductCategory
        {
            Name = dto.Name,
            Sequence = dto.Sequence,
            ParentId = dto.ParentId,
        };

        if (dto.Active.HasValue)
        {
            entity.Active = dto.Active.Value;
        }

        await repo.AddAsync(entity);
        return Result<ProductCategory>.Ok(entity);
    }

    public async Task<Result<ProductCategory>> UpdateAsync(Guid id, ProductCategoryUpdateDto dto)
    {
        var existing = await repo.GetByIdAsync(id);
        if (existing is null)
            return Result<ProductCategory>.Err("Category not found!");

        existing.Name = dto.Name;
        existing.Active = dto.Active;
        existing.Sequence = dto.Sequence;
        existing.ParentId = dto.ParentId;
        
        await repo.UpdateAsync(existing);
        return Result<ProductCategory>.Ok(existing);
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        var existing = await repo.GetByIdAsync(id);
        if (existing is null)
            return Result<bool>.Err("Category not found!");

        await repo.DeleteAsync(existing);
        return Result<bool>.Ok(true);
    }
}
