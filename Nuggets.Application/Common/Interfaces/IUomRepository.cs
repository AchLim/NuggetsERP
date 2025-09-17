using Nuggets.Domain.Entities;

namespace Nuggets.Application.Common.Interfaces;

public interface IUomRepository : IGenericRepository<UnitOfMeasure>
{
    Task<UnitOfMeasure?> GetByAbbreviationAsync(string abbreviation, CancellationToken ct = default);
}