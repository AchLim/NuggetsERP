using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Nuggets.Infrastructure.Identity;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace Nuggets.API.Controllers;

[ApiController]
[Route("v1/[controller]")]
[Authorize(Policy = "USERS:READ")]
public class UsersController(
    UserManager<AppUser> userManager,
    RoleManager<AppRole> roleManager
) : ControllerBase
{
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly RoleManager<AppRole> _roleManager = roleManager;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userManager.Users.ToListAsync();
        var result = new List<object>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);

            result.Add(new {
                user.Id,
                user.UserName,
                user.Email,
                Roles = roles,
                Permissions = claims.Where(c => c.Type == "permission").Select(c => c.Value).ToList()
            });
        }
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);

        return Ok(new {
            user.Id,
            user.UserName,
            user.Email,
            Roles = roles,
            Permissions = claims.Where(c => c.Type == "permission").Select(c => c.Value).ToList()
        });
    }
    
    [HttpPost]
    [Authorize(Policy = "USERS:CREATE")]
    public async Task<IActionResult> Create(UserCreateDto dto)
    {
        var user = new AppUser
        {
            UserName = dto.UserName,
            Email = dto.Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        if (dto.Roles?.Any() == true)
            await _userManager.AddToRolesAsync(user, dto.Roles);

        if (dto.Permissions?.Any() != true) return Ok(new { user.Id, user.UserName, user.Email });
        
        foreach (var perm in dto.Permissions)
            await _userManager.AddClaimAsync(user, new Claim("permission", perm));

        return Ok(new { user.Id, user.UserName, user.Email });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "USERS:UPDATE")]
    public async Task<IActionResult> Update(Guid id, UserUpdateDto dto)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound();

        user.UserName = dto.UserName;
        user.Email = dto.Email;
        await _userManager.UpdateAsync(user);

        // Update roles
        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRolesAsync(user, dto.Roles);

        // Update permissions
        var currentClaims = await _userManager.GetClaimsAsync(user);
        var permissionClaims = currentClaims.Where(c => c.Type == "permission");
        foreach (var c in permissionClaims)
            await _userManager.RemoveClaimAsync(user, c);

        foreach (var perm in dto.Permissions)
            await _userManager.AddClaimAsync(user, new Claim("permission", perm));

        return Ok(new { success = true });
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "USERS:DELETE")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);

        // ❌ Protect: don’t delete Admin users
        if (roles.Contains("Admin"))
            return BadRequest("Cannot delete Admin users");

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new { success = true });
    }
}


public record UserCreateDto(string UserName, string Email, string Password, List<string> Roles, List<string> Permissions);
public record UserUpdateDto(string UserName, string Email, List<string> Roles, List<string> Permissions);