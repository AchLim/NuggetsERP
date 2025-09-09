using Microsoft.EntityFrameworkCore;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Domain.Entities;
using Nuggets.Infrastructure.Persistence;

namespace Nuggets.Infrastructure.Repositories
{
    public class FoodRecipeRepository(NuggetsDbContext db)
        : GenericRepository<FoodRecipe>(db), IFoodRecipeRepository
    {
        private readonly NuggetsDbContext _db = db;
        public async Task<IReadOnlyList<FoodRecipe>> GetByProductIdAsync(Guid productId, CancellationToken ct = default)
        {
            return await _db.FoodRecipes
                .Include(fr => fr.FoodMaterial)
                .Include(fr => fr.Uom)
                .Where(fr => fr.ProductId == productId)
                .AsNoTracking()
                .ToListAsync(ct);
        }
    }
}