using Nuggets.Domain.Entities;

namespace Nuggets.Application.Common.Interfaces;

public interface ICustomerRepository : IGenericRepository<Customer>
{
    Task<Customer?> GetByEmailAsync(string email, CancellationToken ct = default);
}
