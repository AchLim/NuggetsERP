using Microsoft.EntityFrameworkCore;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Domain.Entities;
using Nuggets.Infrastructure.Persistence;

namespace Nuggets.Infrastructure.Repositories;

public sealed class DeliveryNoteRepository(NuggetsDbContext db)
    : GenericRepository<DeliveryNote>(db), IDeliveryNoteRepository
{
    private readonly NuggetsDbContext _db = db;

    public async Task<DeliveryNote?> GetByIdWithLinesAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.DeliveryNotes
            .Include(grn => grn.Lines)
            .ThenInclude(line => line.Product)
            .Include(grn => grn.SalesOrder)
            .FirstOrDefaultAsync(grn => grn.Id == id, ct);
    }
}