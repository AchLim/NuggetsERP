using Nuggets.Application.Common.Interfaces;
using Nuggets.Domain.Entities;
using Nuggets.Infrastructure.Persistence;

namespace Nuggets.Infrastructure.Repositories;

public sealed class ProductUomRepository(NuggetsDbContext db) : GenericRepository<ProductUom>(db), IProductUomRepository
{
    
}
