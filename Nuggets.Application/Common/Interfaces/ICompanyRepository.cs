using Nuggets.Domain.Entities;

namespace Nuggets.Application.Common.Interfaces;

public interface ICompanyRepository : IGenericRepository<Company>
{
    Task<IReadOnlyList<Company>> GetUserCompaniesAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<Guid>> GetUserCompanyIdsAsync(Guid userId, CancellationToken ct = default);
}