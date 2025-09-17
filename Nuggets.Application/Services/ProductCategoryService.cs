using Nuggets.Application.Common;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Application.Common.Services;
using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Services;

public class ProductCategoryService(IProductCategoryRepository repo) : IProductCategoryService
{
    public async Task<Result<PagedResult<ProductCategory>>> GetPagedAsync(
        int page, int pageSize)
    {
        var (items, totalCount) = await repo.GetPagedAsync(page, pageSize);
        return Result<PagedResult<ProductCategory>>.Ok(new PagedResult<ProductCategory>(items, totalCount, page, pageSize));
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
            : Result<ProductCategory>.Err("Category not found", "NOT_FOUND");
    }

    public async Task<Result<ProductCategory>> CreateAsync(ProductCategoryCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return Result<ProductCategory>.Err("Category name is required!", "VALIDATION_ERROR");

        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var entity = new ProductCategory
            {
                Name = dto.Name,
                Sequence = dto.Sequence,
                ParentId = dto.ParentId,
                Active = dto.Active ?? true
            };

            await repo.AddAsync(entity);
            await tx.CommitAsync();

            return Result<ProductCategory>.Ok(entity);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<ProductCategory>.Err($"Failed to create category: {ex.Message}", "DB_ERROR");
        }
    }

    public async Task<Result<ProductCategory>> UpdateAsync(Guid id, ProductCategoryUpdateDto dto)
    {
        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing is null)
                return Result<ProductCategory>.Err("Category not found", "NOT_FOUND");

            existing.Name = dto.Name;
            existing.Active = dto.Active;
            existing.Sequence = dto.Sequence;
            existing.ParentId = dto.ParentId;

            await repo.UpdateAsync(existing);
            await tx.CommitAsync();
            return Result<ProductCategory>.Ok(existing);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<ProductCategory>.Err($"Failed to update category: {ex.Message}", "DB_ERROR");
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing is null)
                return Result<bool>.Err("Category not found", "NOT_FOUND");

            await repo.DeleteAsync(existing);
            await tx.CommitAsync();
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<bool>.Err($"Failed to delete category: {ex.Message}", "DB_ERROR");
        }
    }
}