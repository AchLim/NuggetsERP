using Nuggets.Domain.Entities;

namespace Nuggets.Application.Common.Interfaces;

public interface IVendorRepository : IGenericRepository<Vendor>
{
    Task<Vendor?> GetByEmailAsync(string email, CancellationToken ct = default);
}
