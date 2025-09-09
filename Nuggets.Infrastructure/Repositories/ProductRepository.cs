using Microsoft.EntityFrameworkCore;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Domain.Entities;
using Nuggets.Infrastructure.Persistence;

namespace Nuggets.Infrastructure.Repositories;

public class ProductRepository(NuggetsDbContext db) : GenericRepository<Product>(db), IProductRepository
{
    private readonly NuggetsDbContext _db = db;

    public async Task<IReadOnlyList<Product>> GetByCategoryIdAsync(Guid categoryId, CancellationToken ct = default)
    {
        return await _db.Products
            .Where(p => p.ProductCategoryId == categoryId)
            .AsNoTracking()
            .ToListAsync(ct);
    }
}
