using Microsoft.EntityFrameworkCore;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Domain.Entities;
using Nuggets.Infrastructure.Persistence;

namespace Nuggets.Infrastructure.Repositories
{
    public class ProductCategoryRepository(NuggetsDbContext db)
        : GenericRepository<ProductCategory>(db: db), IProductCategoryRepository
    {
        private readonly NuggetsDbContext _db = db;
        
        public new async Task<IReadOnlyList<ProductCategory>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.ProductCategories.Include(pc => pc.Parent).AsNoTracking().ToListAsync(ct);
        }
    }
}