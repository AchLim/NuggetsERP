    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using System.Security.Cryptography;
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

            private const int AccessTokenMinutes = 5;
            private const int RefreshTokenDays = 7;

            [HttpPost("login")]
            public async Task<IActionResult> Login(LoginDto dto)
            {
                var user = await userManager.FindByEmailAsync(dto.Email)
                           ?? await userManager.FindByNameAsync(dto.Email);

                if (user == null || !await userManager.CheckPasswordAsync(user, dto.Password))
                    return Unauthorized();

                var roles = await userManager.GetRolesAsync(user);

                var accessToken = await GenerateJwtToken(user, roles, TimeSpan.FromMinutes(AccessTokenMinutes));
                var refreshToken = GenerateRefreshToken();

                // store refresh token in DB
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(RefreshTokenDays);
                await userManager.UpdateAsync(user);

                // Always set companies (⚠️ no check if more than one)
                var userCompaniesResult = await companyService.GetUserCompaniesAsync(user.Id);
                if (userCompaniesResult.IsSuccess && userCompaniesResult.Value!.Any())
                {
                    await companyService.SetActiveCompaniesAsync(user.Id,
                        userCompaniesResult.Value!.Select(c => c.Id));
                }

                // Set cookies
                SetCookie("jwt", accessToken, TimeSpan.FromMinutes(AccessTokenMinutes));
                SetCookie("refresh_token", refreshToken, TimeSpan.FromDays(RefreshTokenDays));

                return Ok(new { success = true });
            }

            [HttpPost("logout")]
            public IActionResult Logout()
            {
                Response.Cookies.Delete("jwt");
                Response.Cookies.Delete("refresh_token");
                Response.Cookies.Delete("active_companies");
                return Ok(new { success = true });
            }

            [HttpPost("refresh")]
            public async Task<IActionResult> Refresh()
            {
                if (!Request.Cookies.TryGetValue("refresh_token", out var refreshToken))
                    return Unauthorized();

                var user = await userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
                if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
                    return Unauthorized();

                var roles = await userManager.GetRolesAsync(user);
                var newAccessToken = await GenerateJwtToken(user, roles, TimeSpan.FromMinutes(AccessTokenMinutes));

                SetCookie("jwt", newAccessToken, TimeSpan.FromMinutes(AccessTokenMinutes));

                return Ok(new { success = true });
            }

            [HttpPost("verify-age")]
            public IActionResult VerifyAge([FromForm] bool verified)
            {
                if (!verified) return BadRequest(new { success = false });
                Response.Cookies.Append("age_verified", "true", new CookieOptions
                {
                    HttpOnly = false,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddDays(30)
                });

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

            private async Task<string> GenerateJwtToken(AppUser user, IList<string> roles, TimeSpan expiry)
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
                    expires: DateTime.UtcNow.Add(expiry),
                    signingCredentials: creds
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }

            private string GenerateRefreshToken()
            {
                return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            }

            private void SetCookie(string key, string value, TimeSpan expires)
            {
                Response.Cookies.Append(key, value, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.Add(expires)
                });
            }
        }

        public record LoginDto(string Email, string Password);
    }