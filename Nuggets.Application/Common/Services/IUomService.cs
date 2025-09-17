using Nuggets.Application.DTOs;
using Nuggets.Application.Services;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Common.Services;

public interface IUomService
{
    Task<Result<PagedResult<UnitOfMeasureListDto>>> GetPagedAsync(int page, int pageSize);

    /// <summary>
    /// Get all UOMs.
    /// </summary>
    Task<IReadOnlyList<UnitOfMeasure>> GetAllAsync();

    /// <summary>
    /// Get a UOM by abbreviation (e.g. "kg") or Id.
    /// </summary>
    Task<UnitOfMeasure?> GetByIdAsync(Guid id);

    Task<UnitOfMeasure?> GetByAbbreviationAsync(string abbreviation);

    /// <summary>
    /// Convert a value between two UOMs if a direct or bidirectional conversion exists.
    /// </summary>
    /// <returns>Null if no conversion found, otherwise converted value.</returns>
    Task<decimal?> ConvertAsync(Guid fromUomId, Guid toUomId, decimal value, UomService.ConversionPurpose purpose);

    /// <summary>
    /// Find all conversions for a given unit.
    /// </summary>
    Task<IReadOnlyList<UnitOfMeasureConversion>> GetConversionsAsync(Guid uomId);

    Task<(decimal qtyInBase, decimal unitCostInBase)> ConvertLineAsync(
        Guid productId,
        Guid fromUomId,
        decimal qty,
        decimal unitCost,
        CancellationToken ct = default
    );
}