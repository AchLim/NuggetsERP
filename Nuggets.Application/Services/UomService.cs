using Nuggets.Application.Common;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Application.Common.Services;
using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Services;

public class UomService(
    IUomRepository uomRepo,
    IUomConversionsRepository convRepo,
    IProductRepository productRepo
    ) : IUomService
{
    public async Task<Result<PagedResult<UnitOfMeasureListDto>>> GetPagedAsync(int page, int pageSize)
    {
        var (items, totalCount) = await uomRepo.GetPagedAsync(page, pageSize);

        var list = items.Select(ToListDto).ToList();
        return Result<PagedResult<UnitOfMeasureListDto>>.Ok(new PagedResult<UnitOfMeasureListDto>(list, totalCount, page, pageSize));
    }

    public async Task<IReadOnlyList<UnitOfMeasure>> GetAllAsync()
        => await uomRepo.GetAllAsync();

    public async Task<UnitOfMeasure?> GetByIdAsync(Guid id)
        => await uomRepo.GetByIdAsync(id);

    public async Task<UnitOfMeasure?> GetByAbbreviationAsync(string abbreviation)
        => await uomRepo.GetByAbbreviationAsync(abbreviation);

    public async Task<decimal?> ConvertAsync(Guid fromUomId, Guid toUomId, decimal value, ConversionPurpose purpose)
    {
        if (fromUomId == toUomId) return value;
        
        var conv = await convRepo.GetConversionAsync(fromUomId, toUomId);
        if (conv == null) return null;
        

        bool isDirect = conv.FromUomId == fromUomId && conv.ToUomId == toUomId;
        bool isReverse = conv.IsBidirectional && conv.FromUomId == toUomId && conv.ToUomId == fromUomId;

        if (purpose == ConversionPurpose.Quantity)
        {
            if (isDirect) return value * conv.ConversionRate;
            if (isReverse) return value / conv.ConversionRate;
        }
        else if (purpose == ConversionPurpose.Price)
        {
            if (isDirect) return value / conv.ConversionRate;
            if (isReverse) return value * conv.ConversionRate;
        }
        
        return null;
    }

    public async Task<IReadOnlyList<UnitOfMeasureConversion>> GetConversionsAsync(Guid uomId)
        => await convRepo.GetConversionsForUomAsync(uomId);

    public async Task<(decimal qtyInBase, decimal unitCostInBase)> ConvertLineAsync(
        Guid productId,
        Guid fromUomId,
        decimal qty,
        decimal unitCost,
        CancellationToken ct = default)
    {
        var product = await productRepo.GetByIdAsync(productId, ct)
                      ?? throw new InvalidOperationException($"Product {productId} not found");

        // If same UOM, nothing to convert
        if (fromUomId == product.UomId)
            return (qty, unitCost);

        // Convert quantity from fromUOM -> baseUOM
        var qtyInBase = await ConvertAsync(fromUomId, product.UomId, qty, ConversionPurpose.Quantity);
        if (qtyInBase == null)
            throw new InvalidOperationException($"No valid conversion from {fromUomId} -> {product.UomId} for qty");

        // Convert unit cost to be expressed per base UOM
        var unitCostInBase = await ConvertAsync(fromUomId, product.UomId, unitCost, ConversionPurpose.Price);
        if (unitCostInBase == null)
            throw new InvalidOperationException($"No valid conversion from {fromUomId} -> {product.UomId} for cost");

        return (qtyInBase.Value, unitCostInBase.Value);
    }

    
    private static UnitOfMeasureListDto ToListDto(UnitOfMeasure p) => 
        new UnitOfMeasureListDto(p.Id, p.Name, p.Abbreviation);
    
    public enum ConversionPurpose
    {
        Quantity,
        Price
    }
}