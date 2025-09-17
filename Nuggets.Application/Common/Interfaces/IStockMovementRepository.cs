using Nuggets.Domain.Entities;

namespace Nuggets.Application.Common.Interfaces;

public interface IStockMovementRepository : IGenericRepository<StockMovement>
{
    Task<IReadOnlyList<StockMovement>> GetByProductAsync(Guid productId, CancellationToken ct = default);

    Task<IReadOnlyList<StockMovement>> GetByReferenceAsync(Guid referenceId, string referenceType,
        CancellationToken ct = default);
    Task<decimal> GetNetQuantityAsync(Guid productId, CancellationToken ct = default);
}