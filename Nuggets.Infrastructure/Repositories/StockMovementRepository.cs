using Microsoft.EntityFrameworkCore;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Domain.Entities;
using Nuggets.Infrastructure.Persistence;

namespace Nuggets.Infrastructure.Repositories;

public class StockMovementRepository(NuggetsDbContext db)
    : GenericRepository<StockMovement>(db), IStockMovementRepository
{
    private readonly NuggetsDbContext _db = db;

    public override async Task<IReadOnlyList<StockMovement>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.StockMovements
            .Include(sm => sm.Product)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<StockMovement>> GetByProductAsync(Guid productId, CancellationToken ct = default)
    {
        return await _db.StockMovements
            .Where(m => m.ProductId == productId)
            .OrderByDescending(m => m.MovementDate)
            .AsNoTracking()
            .ToListAsync(ct);
    }
    
    public async Task<IReadOnlyList<StockMovement>> GetByReferenceAsync(Guid referenceId, string referenceType, CancellationToken ct = default)
    {
        return await db.StockMovements
            .Where(sm => sm.ReferenceId == referenceId && sm.ReferenceType == referenceType)
            .Include(sm => sm.Product)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<decimal> GetNetQuantityAsync(Guid productId, CancellationToken ct = default)
    {
        var inQty = await db.StockMovements
            .Where(sm => sm.ProductId == productId && sm.MovementType == StockMovementType.Inbound)
            .SumAsync(sm => (decimal?)sm.Quantity, ct) ?? 0;

        var outQty = await db.StockMovements
            .Where(sm => sm.ProductId == productId && sm.MovementType == StockMovementType.Outbound)
            .SumAsync(sm => (decimal?)sm.Quantity, ct) ?? 0;

        return inQty - outQty;
    }
}