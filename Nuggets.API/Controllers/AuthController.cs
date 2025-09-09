using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Nuggets.Application.Common.Services;
using Nuggets.Domain.Authorization;
using Nuggets.Infrastructure.Identity;
using Nuggets.Infrastructure.Options;
using Nuggets.Infrastructure.Persistence;

namespace Nuggets.API.Controllers
{
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class AuthController(
        ILogger<AuthController> logger,
        UserManager<AppUser> userManager,
        IOptions<JwtOptions> jwtOptions,
        ICompanyService companyService)
        : ControllerBase
    {
        private readonly ILogger<AuthController> _logger = logger;
        private readonly JwtOptions _jwtOptions = jwtOptions.Value;

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await userManager.FindByEmailAsync(dto.Email)
                       ?? await userManager.FindByNameAsync(dto.Email);

            if (user == null || !await userManager.CheckPasswordAsync(user, dto.Password))
            {
                return Unauthorized();
            }

            var roles = await userManager.GetRolesAsync(user);
            var token = await GenerateToken(user, roles);

            // Always set companies (⚠️ no check if more than one)
            var userCompaniesResult = await companyService.GetUserCompaniesAsync(user.Id);
            if (userCompaniesResult.IsSuccess && userCompaniesResult.Value!.Any())
            {
                await companyService.SetActiveCompaniesAsync(user.Id,
                    userCompaniesResult.Value!.Select(c => c.Id));
            }

            // JWT HttpOnly cookie
            Response.Cookies.Append("jwt", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiryMinutes)
            });

            return Ok(new { success = true });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt");
            Response.Cookies.Delete("active_companies");
            return Ok(new { success = true });
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            if (User.Identity?.IsAuthenticated != true)
                return Unauthorized();

            var identity = User.Identity as ClaimsIdentity;
            var user = new
            {
                User.Identity?.Name,
                Roles = identity?.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList() ?? [],
                Permissions = identity?.Claims
                    .Where(c => c.Type == "permission")
                    .Select(c => c.Value)
                    .ToList() ?? []
            };
            return Ok(user);
        }

        private async Task<string> GenerateToken(AppUser user, IList<string> roles)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? "unknown")
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            // Role-based permissions
            var rolePermissions = roles
                .SelectMany(r =>
                    RolePermissions.PermissionsByRole
                        .Where(kv => string.Equals(kv.Key, r, StringComparison.OrdinalIgnoreCase))
                        .SelectMany(kv => kv.Value))
                .Distinct();

            claims.AddRange(rolePermissions.Select(p => new Claim("permission", p)));

            // Explicit user claims
            var userClaims = await userManager.GetClaimsAsync(user);
            var explicitPermissions = userClaims.Where(c => c.Type == "permission").Select(c => c.Value);
            claims.AddRange(explicitPermissions.Select(p => new Claim("permission", p)));

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public record LoginDto(string Email, string Password);
}