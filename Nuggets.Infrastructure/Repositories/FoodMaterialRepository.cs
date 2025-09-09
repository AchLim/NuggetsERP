using Nuggets.Application.Common.Interfaces;
using Nuggets.Domain.Entities;
using Nuggets.Infrastructure.Persistence;

namespace Nuggets.Infrastructure.Repositories;

public class FoodMaterialRepository(NuggetsDbContext db) : GenericRepository<FoodMaterial>(db), IFoodMaterialRepository
{
    private readonly NuggetsDbContext _db = db;
}