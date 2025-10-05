using System.Linq.Dynamic.Core;
using System.Reflection;
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
    public async Task<Product?> GetWithMovementsAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Products
            .Include(p => p.StockMovements)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public override async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Products
            .Include(p => p.StockMovements)
            .Include(p => p.Uom)
            .Include(p => p.ProductCategory)
            .Include(p => p.Vendor)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public override Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, IQueryable<Product>? startingQuery = null, CancellationToken ct = default)
    {
        startingQuery = _db.Products
            .Include(p => p.StockMovements)
            .Include(p => p.Uom)
            .Include(p => p.Vendor)
            .Include(p => p.ProductCategory)
            .AsNoTracking()
            .AsQueryable();
        
        return base.GetPagedAsync(page, pageSize, startingQuery, ct);
    }
}
