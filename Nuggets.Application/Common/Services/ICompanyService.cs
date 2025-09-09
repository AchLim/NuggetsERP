using Microsoft.AspNetCore.Http;
using Nuggets.Application.Common;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Common.Services;

public interface ICompanyService
{
    Task<Result<IReadOnlyList<Company>>> GetUserCompaniesAsync(Guid userId);
    Task<Result<IReadOnlyList<Guid>>> SetActiveCompaniesAsync(Guid userId, IEnumerable<Guid> companyIds);
    Task<Result<IReadOnlyList<Guid>>> GetActiveCompaniesAsync(HttpRequest request);
}