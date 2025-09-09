using Microsoft.AspNetCore.Http;
using Nuggets.Application.Common.Interfaces;
using System.Security.Claims;

namespace Nuggets.Infrastructure.Identity;

public class UserContext(IHttpContextAccessor accessor) : IUserContext
{
    public Guid UserId =>
        Guid.Parse(accessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public IEnumerable<Guid> CompanyIds =>
        accessor.HttpContext!.User.FindAll("company_ids").Select(c => Guid.Parse(c.Value));

    public IEnumerable<Guid> ActiveCompanyIds
    {
        get
        {
            var cookie = accessor.HttpContext?.Request.Cookies["active_companies"];
            if (string.IsNullOrWhiteSpace(cookie)) return Enumerable.Empty<Guid>();

            return cookie.Split(',')
                .Select(x => Guid.TryParse(x, out var g) ? g : Guid.Empty)
                .Where(g => g != Guid.Empty);
        }
    }

    public bool IsInRole(string role) =>
        accessor.HttpContext!.User.IsInRole(role);
}