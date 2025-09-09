using Microsoft.AspNetCore.Mvc;
using Nuggets.Application.Services;

namespace Nuggets.API.Controllers;

[ApiController]
[Route("v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class ReportsController : ControllerBase
{
    private readonly IReportingService _reportingService;

    public ReportsController(IReportingService reportingService)
    {
        _reportingService = reportingService;
    }

    [HttpGet("net-profit")]
    public async Task<IActionResult> GetNetProfit([FromQuery] DateTime start, [FromQuery] DateTime end)
    {
        var profit = await _reportingService.CalculateNetProfitAsync(start, end);
        return Ok(new { NetProfit = profit });
    }
}
