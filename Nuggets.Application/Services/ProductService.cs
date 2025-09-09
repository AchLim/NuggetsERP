using Nuggets.Application.Common;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Application.Common.Services;
using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Services;

public sealed class ProductService(IProductRepository repo) : IProductService
{
    public async Task<Result<PagedResult<Product>>> GetPagedAsync(int page, int pageSize, IDictionary<string, string?>? filters, string? sort)
    {
        var (items, totalCount) = await repo.GetPagedAsync(page, pageSize, filters, sort);
        var result = new PagedResult<Product>(items, totalCount, page, pageSize);
        return Result<PagedResult<Product>>.Ok(result);
    }
    
    public async Task<Result<Product>> CreateAsync(ProductCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return Result<Product>.Err("Full name is required!");

        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Stock = dto.Stock,
            SupplierId = dto.SupplierId,
            ProductCategoryId = dto.ProductCategoryId,

        };

        await repo.AddAsync(product);
        return Result<Product>.Ok(product);
    }

    public async Task<Result<IReadOnlyList<Product>>> GetAllAsync()
    {
        var list = await repo.GetAllAsync();
        return Result<IReadOnlyList<Product>>.Ok(list);
    }

    public async Task<Result<Product>> GetByIdAsync(Guid id)
    {
        var product = await repo.GetByIdAsync(id);
        return product is not null
            ? Result<Product>.Ok(product)
            : Result<Product>.Err("Product not found!");
    }

    public async Task<Result<Product>> UpdateAsync(Guid id, ProductUpdateDto dto)
    {
        var existing = await repo.GetByIdAsync(id);

        if (existing is null)
            return Result<Product>.Err("Product not found!");

        existing.Name = dto.Name;
        existing.Description = dto.Description;
        existing.Price = dto.Price;
        existing.Stock = dto.Stock;
        existing.SupplierId = dto.SupplierId;
        existing.ProductCategoryId = dto.ProductCategoryId;

        await repo.UpdateAsync(existing);
        return Result<Product>.Ok(existing);
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        var existing = await repo.GetByIdAsync(id);
        if (existing is null)
            return Result<bool>.Err("Product not found!");

        await repo.DeleteAsync(existing);
        return Result<bool>.Ok(true);
    }
}
