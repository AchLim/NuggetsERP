using Microsoft.AspNetCore.Http;
using Nuggets.Application.Common;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Application.Common.Services;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Services;

public sealed class CompanyService(
    ICompanyRepository repo,
    IHttpContextAccessor httpContextAccessor
) : ICompanyService
{
    public async Task<Result<IReadOnlyList<Company>>> GetUserCompaniesAsync(Guid userId)
    {
        var companies = await repo.GetUserCompaniesAsync(userId);
        return Result<IReadOnlyList<Company>>.Ok(companies);
    }

    public async Task<Result<IReadOnlyList<Guid>>> SetActiveCompaniesAsync(Guid userId, IEnumerable<Guid> companyIds)
    {
        // Validate user-company membership
        var validCompanyIds = await repo.GetUserCompanyIdsAsync(userId);

        var filtered = companyIds.Where(id => validCompanyIds.Contains(id)).ToList();
    
        var ctx = httpContextAccessor.HttpContext!;
        
        if (!filtered.Any())
        {
            ctx.Response.Cookies.Delete("active_companies");
            return Result<IReadOnlyList<Guid>>.Ok(new List<Guid>());
        }

        var joined = string.Join(",", filtered);

        ctx.Response.Cookies.Append("active_companies", joined, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddHours(8)
        });

        return Result<IReadOnlyList<Guid>>.Ok(filtered);
    }

    public Task<Result<IReadOnlyList<Guid>>> GetActiveCompaniesAsync(HttpRequest request)
    {
        var cookie = request.Cookies["active_companies"];
        if (string.IsNullOrWhiteSpace(cookie))
            return Task.FromResult(Result<IReadOnlyList<Guid>>.Ok(new List<Guid>()));

        var ids = cookie.Split(',')
            .Select(x => Guid.TryParse(x, out var g) ? g : Guid.Empty)
            .Where(g => g != Guid.Empty)
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<Guid>>.Ok(ids));
    }
}