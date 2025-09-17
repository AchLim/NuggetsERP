using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Application.Common.Services;
using Nuggets.Domain.Entities;

namespace Nuggets.API.Controllers;

[ApiController]
[Route("v{version:apiVersion}/inventory")]
[ApiVersion("1.0")]
public class InventoryController(
    ILogger<InventoryController> logger,
    IInventoryService inventoryService
) : ControllerBase
{
    [HttpGet("movements")]
    [Authorize(Policy = "INVENTORY:READ")]
    public async Task<IActionResult> GetAllMovements(CancellationToken ct)
    {
        var result = await inventoryService.GetAllMovementsAsync(ct);
        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error, code = result.ErrorCode });
    }

    [HttpGet("product/{productId:guid}/movements")]
    [Authorize(Policy = "INVENTORY:READ")]
    public async Task<IActionResult> GetMovements(Guid productId, CancellationToken ct)
    {
        var result = await inventoryService.GetProductMovementsAsync(productId, ct);
        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(new { error = result.Error, code = result.ErrorCode });
    }
}