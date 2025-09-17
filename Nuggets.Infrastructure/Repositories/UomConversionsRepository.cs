using Microsoft.EntityFrameworkCore;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Domain.Entities;
using Nuggets.Infrastructure.Persistence;

namespace Nuggets.Infrastructure.Repositories;

public class UomConversionsRepository(NuggetsDbContext db) : GenericRepository<UnitOfMeasureConversion>(db), IUomConversionsRepository
{
    private readonly NuggetsDbContext _db = db;

    public async Task<UnitOfMeasureConversion?> GetConversionAsync(Guid fromUomId, Guid toUomId, CancellationToken ct = default)
    {
        return await _db.UomConversions
            .FirstOrDefaultAsync(c =>
                (c.FromUomId == fromUomId && c.ToUomId == toUomId)
                || (c.IsBidirectional && c.FromUomId == toUomId && c.ToUomId == fromUomId), ct);
    }

    public async Task<IReadOnlyList<UnitOfMeasureConversion>> GetConversionsForUomAsync(Guid uomId, CancellationToken ct = default)
    {
        return await _db.UomConversions
            .Include(c => c.FromUom)
            .Include(c => c.ToUom)
            .Where(c => c.FromUomId == uomId || c.ToUomId == uomId)
            .AsNoTracking()
            .ToListAsync(ct);
    }
}