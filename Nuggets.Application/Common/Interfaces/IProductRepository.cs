using Nuggets.Domain.Entities;

namespace Nuggets.Application.Common.Interfaces
;
public interface IProductRepository : IGenericRepository<Product>
{
    Task<IReadOnlyList<Product>> GetByCategoryIdAsync(Guid categoryId, CancellationToken ct = default);
    Task<Product?> GetWithMovementsAsync(Guid id, CancellationToken ct = default);
}
