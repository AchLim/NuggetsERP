using Microsoft.EntityFrameworkCore;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Domain.Entities;
using Nuggets.Infrastructure.Persistence;

namespace Nuggets.Infrastructure.Repositories;

public sealed class JournalEntryRepository(NuggetsDbContext db)
    : GenericRepository<JournalEntry>(db), IJournalEntryRepository
{
    public override async Task<(IReadOnlyList<JournalEntry> Items, int TotalCount)> GetPagedAsync(int page, int pageSize,
        IQueryable<JournalEntry>? startingQuery = null, CancellationToken ct = default)
    {
        startingQuery = db.JournalEntries
            .Include(je => je.Items)
            .AsNoTracking()
            .AsQueryable();
        return await base.GetPagedAsync(page, pageSize, startingQuery, ct);
    }

    public override async Task<JournalEntry?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.JournalEntries
            .Include(je => je.Items)
            .FirstOrDefaultAsync(sr => sr.Id == id, ct);
    }
}