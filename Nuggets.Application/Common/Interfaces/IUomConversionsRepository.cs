using Nuggets.Domain.Entities;

namespace Nuggets.Application.Common.Interfaces;

public interface IUomConversionsRepository : IGenericRepository<UnitOfMeasureConversion>
{
    Task<UnitOfMeasureConversion?> GetConversionAsync(Guid fromUomId, Guid toUomId, CancellationToken ct = default);
    Task<IReadOnlyList<UnitOfMeasureConversion>> GetConversionsForUomAsync(Guid uomId, CancellationToken ct = default);
}