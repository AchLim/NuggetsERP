using Microsoft.EntityFrameworkCore;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Domain.Entities;
using Nuggets.Infrastructure.Persistence;

namespace Nuggets.Infrastructure.Repositories
{
    public class CustomerRepository(NuggetsDbContext db) : GenericRepository<Customer>(db), ICustomerRepository
    {
        private readonly NuggetsDbContext _db = db;

        public async Task<Customer?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            return await _db.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Email != null && c.Email.ToLower() == email.ToLower(), ct);
        }
    }
}