using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Application.Common.Services;
using Nuggets.Application.DTOs;
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
    
    [HttpGet("adjustments")]
    [Authorize(Policy = "INVENTORY:READ")]
    public async Task<IActionResult> GetAdjustments(CancellationToken ct)
    {
        var result = await inventoryService.GetAllMovementsAsync(ct);

        if (!result.IsSuccess) return BadRequest(new { error = result.Error });

        var adjustments = result.Value
            .Where(m => m.MovementType == StockMovementType.Adjustment)
            .OrderByDescending(m => m.MovementDate)
            .ToList();

        return Ok(new { items = adjustments, totalCount = adjustments.Count });
    }

    [HttpPost("adjustments")]
    [Authorize(Policy = "INVENTORY:ADJUST")]
    public async Task<IActionResult> ApplyAdjustment([FromBody] InventoryAdjustmentDto dto, CancellationToken ct)
    {
        var result = await inventoryService.ApplyInventoryAdjustmentAsync(
            dto.ProductId,
            dto.Quantity,
            dto.UnitCost,
            ct
        );

        return result.IsSuccess
            ? Ok(new { success = true })
            : BadRequest(new { error = result.Error, code = result.ErrorCode });
    }

    [HttpPost("adjustments/{adjustmentId:guid}/revert")]
    [Authorize(Policy = "INVENTORY:ADJUST")]
    public async Task<IActionResult> RevertAdjustment(Guid adjustmentId, CancellationToken ct)
    {
        var result = await inventoryService.RevertInventoryAdjustmentAsync(adjustmentId, ct);
        return result.IsSuccess
            ? Ok(new { success = true })
            : BadRequest(new { error = result.Error, code = result.ErrorCode });
    }
    
    [HttpGet("adjustments/{id:guid}")]
    [Authorize(Policy = "INVENTORY:READ")]
    public async Task<IActionResult> GetAdjustment(Guid id, CancellationToken ct)
    {
        var result = await inventoryService.GetInventoryAdjustmentAsync(id, ct);
        if (!result.IsSuccess) 
            return NotFound(new { error = result.Error, code = result.ErrorCode });

        return Ok(result.Value);
    }
    
    [HttpDelete("adjustments/{id:guid}")]
    [Authorize(Policy = "INVENTORY:ADJUST")]
    public async Task<IActionResult> DeleteAdjustment(Guid id, CancellationToken ct)
    {
        var result = await inventoryService.DeleteInventoryAdjustmentAsync(id, ct);
        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error, code = result.ErrorCode });
    }
}