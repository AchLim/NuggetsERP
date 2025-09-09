using Nuggets.Domain.Entities;

namespace Nuggets.Application.Common.Interfaces;

public interface ISupplierRepository : IGenericRepository<Supplier>
{
    Task<Supplier?> GetByEmailAsync(string email, CancellationToken ct = default);
}
