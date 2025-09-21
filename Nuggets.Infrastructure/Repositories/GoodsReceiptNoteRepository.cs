using Microsoft.EntityFrameworkCore;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Domain.Entities;
using Nuggets.Infrastructure.Persistence;

namespace Nuggets.Infrastructure.Repositories;

public sealed class GoodsReceiptNoteRepository(NuggetsDbContext db)
    : GenericRepository<GoodsReceiptNote>(db), IGoodsReceiptNoteRepository
{
    private readonly NuggetsDbContext _db = db;

    public override async Task<(IReadOnlyList<GoodsReceiptNote> Items, int TotalCount)> GetPagedAsync(int page, int pageSize,
        IQueryable<GoodsReceiptNote>? startingQuery = null, CancellationToken ct = default)
    {
        startingQuery = db.GoodsReceiptNotes
            .Include(ci => ci.PurchaseOrder)
            .ThenInclude(po => po.Vendor)
            .Include(ci => ci.Lines)
            .AsQueryable();
        return await base.GetPagedAsync(page, pageSize, startingQuery, ct);
    }
    
    public async Task<GoodsReceiptNote?> GetByIdWithLinesAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.GoodsReceiptNotes
            .Include(grn => grn.Lines)
            .ThenInclude(line => line.Product)
            .Include(grn => grn.PurchaseOrder)
            .FirstOrDefaultAsync(grn => grn.Id == id, ct);
    }
    
    public async Task<(IReadOnlyList<GoodsReceiptNote> Items, int TotalCount)> GetPagedByPoIdAsync(
        int page, int pageSize, Guid purchaseOrderId, CancellationToken ct = default)
    {
        var query = _db.GoodsReceiptNotes
            .Include(grn => grn.PurchaseOrder)
            .Include(grn => grn.Lines)
            .ThenInclude(line => line.Product)
            .Where(grn => grn.PurchaseOrderId == purchaseOrderId);

        return await GetPagedAsync(page, pageSize, query, ct);
    }
}