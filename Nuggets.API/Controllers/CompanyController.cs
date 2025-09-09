using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nuggets.Application.Common.Services;
using System.Security.Claims;

namespace Nuggets.API.Controllers;

[ApiController]
[Route("v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class CompanyController(ILogger<CompanyController> logger, ICompanyService companyService) : ControllerBase
{
    /// <summary>
    /// Get all companies current user belongs to
    /// </summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMyCompanies(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        logger.LogInformation("user: {}", User);
        logger.LogInformation("userId: {}", userId);
        var result = await companyService.GetUserCompaniesAsync(userId);
        logger.LogInformation("result: {}", result.IsSuccess);
        logger.LogInformation("result: {}", result.Value);

        return result.IsSuccess
            ? Ok(result.Value!.Select(c => new { c.Id, c.Name }))
            : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Set active companies (via HttpOnly cookie)
    /// </summary>
    [HttpPost("set-active")]
    public async Task<IActionResult> SetActive([FromBody] Guid[] companyIds, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await companyService.SetActiveCompaniesAsync(userId, companyIds);

        return result.IsSuccess
            ? Ok(new { activeCompanies = result.Value })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Returns currently active companies from cookie
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var result = await companyService.GetActiveCompaniesAsync(Request);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error });
    }
}