using Microsoft.EntityFrameworkCore;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Domain.Entities;
using Nuggets.Infrastructure.Persistence;

namespace Nuggets.Infrastructure.Repositories
{
    public class VendorRepository(NuggetsDbContext db) : GenericRepository<Vendor>(db), IVendorRepository
    {
        private readonly NuggetsDbContext _db = db;

        public async Task<Vendor?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            return await _db.Vendors
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Email != null && c.Email.ToLower() == email.ToLower(), ct);
        }
    }
}