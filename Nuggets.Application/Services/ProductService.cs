using Nuggets.Application.Common;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Application.Common.Services;
using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Services;

public sealed class ProductService(IProductRepository repo) : IProductService
{
    public async Task<Result<PagedResult<ProductListDto>>> GetPagedAsync(int page, int pageSize)
    {
        var (items, totalCount) = await repo.GetPagedAsync(page, pageSize);

        var list = items.Select(ToListDto).ToList();
        return Result<PagedResult<ProductListDto>>.Ok(new PagedResult<ProductListDto>(list, totalCount, page, pageSize));
    }
    
    public async Task<Result<IReadOnlyList<ProductListDto>>> GetAllAsync()
    {
        var list = await repo.GetAllAsync();
        return Result<IReadOnlyList<ProductListDto>>.Ok(list.Select(ToListDto).ToList());
    }

    public async Task<Result<ProductReadDto>> GetByIdAsync(Guid id)
    {
        var product = await repo.GetByIdAsync(id); 
        return product is not null
            ? Result<ProductReadDto>.Ok(ToReadDto(product))
            : Result<ProductReadDto>.Err("Product not found", "NOT_FOUND");
    }


    public async Task<Result<ProductReadDto>> CreateAsync(ProductCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return Result<ProductReadDto>.Err("Product name is required!", "VALIDATION_ERROR");

        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                UomId = dto.UomId,
                DefaultPurchasePrice = dto.DefaultPurchasePrice,
                DefaultSalesPrice = dto.DefaultSalesPrice,
                VendorId = dto.VendorId,
                ProductCategoryId = dto.ProductCategoryId,
            };

            await repo.AddAsync(product);
            await tx.CommitAsync();

            // Reload product with navigation properties
            var fullProduct = await repo.GetByIdAsync(product.Id);
            
            return Result<ProductReadDto>.Ok(ToReadDto(fullProduct!));
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<ProductReadDto>.Err($"Failed to create product: {ex.Message}", "DB_ERROR");
        }
    }

    public async Task<Result<ProductReadDto>> UpdateAsync(Guid id, ProductUpdateDto dto)
    {
        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing is null)
                return Result<ProductReadDto>.Err("Product not found", "NOT_FOUND");

            existing.Name = dto.Name;
            existing.Description = dto.Description;
            existing.DefaultPurchasePrice = dto.DefaultPurchasePrice;
            existing.DefaultSalesPrice = dto.DefaultSalesPrice;
            existing.VendorId = dto.VendorId;
            existing.ProductCategoryId = dto.ProductCategoryId;
            existing.UomId = dto.UomId;

            await repo.UpdateAsync(existing);
            await tx.CommitAsync();
            return Result<ProductReadDto>.Ok(ToReadDto(existing));
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<ProductReadDto>.Err($"Failed to update product: {ex.Message}", "DB_ERROR");
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing is null)
                return Result<bool>.Err("Product not found", "NOT_FOUND");

            await repo.DeleteAsync(existing);
            await tx.CommitAsync();
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<bool>.Err($"Failed to delete product: {ex.Message}", "DB_ERROR");
        }
    }

    private static ProductListDto ToListDto(Product p) => 
        new ProductListDto(p.Id, p.Name, p.DefaultPurchasePrice, p.DefaultSalesPrice, p.ProductCategory?.Name, p.UomId, p.Uom.Name, p.Vendor?.Name,
            p.StockMovements.Sum(sm =>
                            sm.MovementType == StockMovementType.Inbound ? sm.Quantity :
                            sm.MovementType == StockMovementType.Outbound ? -sm.Quantity :
                            sm.Quantity
            ));

    private static ProductReadDto ToReadDto(Product p) =>
        new ProductReadDto(p.Id, p.Name, p.Description, p.UomId, p.Uom.Name, p.DefaultPurchasePrice, p.DefaultSalesPrice,
            p.ProductCategoryId, p.ProductCategory?.Name, p.VendorId, p.Vendor?.Name,
            p.StockMovements.Sum(sm =>
                sm.MovementType == StockMovementType.Inbound ? sm.Quantity :
                sm.MovementType == StockMovementType.Outbound ? -sm.Quantity :
                sm.Quantity
            ),
            p.StockMovements.Select(sm => new StockMovementReadDto(
                    sm.Id, sm.MovementDate, sm.MovementType.ToString(), sm.Quantity, sm.ReferenceType, sm.ReferenceId))
                .ToList()
        );
}