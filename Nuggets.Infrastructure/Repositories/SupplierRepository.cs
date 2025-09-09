using Microsoft.EntityFrameworkCore;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Domain.Entities;
using Nuggets.Infrastructure.Persistence;

namespace Nuggets.Infrastructure.Repositories
{
    public class SupplierRepository(NuggetsDbContext db) : GenericRepository<Supplier>(db), ISupplierRepository
    {
        private readonly NuggetsDbContext _db = db;

        public async Task<Supplier?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            return await _db.Suppliers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Email != null && c.Email.ToLower() == email.ToLower(), ct);
        }
    }
}