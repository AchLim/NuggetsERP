using Microsoft.EntityFrameworkCore;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Domain.Entities;
using Nuggets.Infrastructure.Persistence;

namespace Nuggets.Infrastructure.Repositories;

public sealed class UomRepository(NuggetsDbContext db) : GenericRepository<UnitOfMeasure>(db), IUomRepository
{
    private readonly NuggetsDbContext _db = db;

    public async Task<UnitOfMeasure?> GetByAbbreviationAsync(string abbreviation, CancellationToken ct = default)
    {
        return await _db.Uoms
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Abbreviation.ToLower() == abbreviation.ToLower(), ct);
    }
}