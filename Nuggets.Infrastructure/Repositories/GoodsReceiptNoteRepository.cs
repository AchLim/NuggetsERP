using Microsoft.EntityFrameworkCore;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Domain.Entities;
using Nuggets.Infrastructure.Persistence;

namespace Nuggets.Infrastructure.Repositories;

public sealed class GoodsReceiptNoteRepository(NuggetsDbContext db)
    : GenericRepository<GoodsReceiptNote>(db), IGoodsReceiptNoteRepository
{
    private readonly NuggetsDbContext _db = db;

    public async Task<GoodsReceiptNote?> GetByIdWithLinesAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.GoodsReceiptNotes
            .Include(grn => grn.Lines)
            .ThenInclude(line => line.Product)
            .Include(grn => grn.PurchaseOrder)
            .FirstOrDefaultAsync(grn => grn.Id == id, ct);
    }
}