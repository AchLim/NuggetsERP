using Nuggets.Domain.Entities;

namespace Nuggets.Application.Common.Interfaces;

public interface IFoodRecipeRepository : IGenericRepository<FoodRecipe>
{
    Task<IReadOnlyList<FoodRecipe>> GetByProductIdAsync(Guid productId, CancellationToken ct = default);
}