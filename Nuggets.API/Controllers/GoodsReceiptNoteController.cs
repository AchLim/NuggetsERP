using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nuggets.Application.Common.Services;
using Nuggets.Application.DTOs;

namespace Nuggets.API.Controllers;

[ApiController]
[Route("v{version:apiVersion}/goods-receipt-note")]
[ApiVersion("1.0")]
public class GoodsReceiptNoteController(ILogger<GoodsReceiptNoteController> logger, IGoodsReceiptNoteService service) : ControllerBase
{
    private readonly ILogger<GoodsReceiptNoteController> _logger = logger;

    [HttpGet]
    [Authorize(Policy = "INVENTORY:READ")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await service.GetPagedAsync(page, pageSize);
        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error, code = result.ErrorCode });
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "INVENTORY:READ")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await service.GetByIdAsync(id);
        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(new { error = result.Error, code = result.ErrorCode });
    }

    [HttpPost]
    [Authorize(Policy = "INVENTORY:READ")]
    public async Task<IActionResult> Create(GoodsReceiptNoteCreateDto dto)
    {
        var result = await service.CreateAsync(dto);
        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error, code = result.ErrorCode });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "INVENTORY:ADJUST")]
    public async Task<IActionResult> Update(Guid id, GoodsReceiptNoteUpdateDto dto)
    {
        var result = await service.UpdateAsync(id, dto);
        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error, code = result.ErrorCode });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "INVENTORY:ADJUST")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await service.DeleteAsync(id);
        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error, code = result.ErrorCode });
    }
}
