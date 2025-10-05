using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nuggets.Application.Common.Services;
using Nuggets.Application.DTOs;

namespace Nuggets.API.Controllers;

[ApiController]
[Route("v{version:apiVersion}/vendor-bill")]
[ApiVersion("1.0")]
public class VendorBillController(ILogger<VendorBillController> logger, IVendorBillService service) : ControllerBase
{
    private readonly ILogger<VendorBillController> _logger = logger;

    [HttpGet]
    [Authorize(Policy = "PURCHASES:READ")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] Guid? purchaseOrderId = null)
    {
        var result = await service.GetPagedAsync(page, pageSize, purchaseOrderId);
        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error, code = result.ErrorCode });
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "PURCHASES:READ")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await service.GetByIdAsync(id);
        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(new { error = result.Error, code = result.ErrorCode });
    }

    [HttpPost]
    [Authorize(Policy = "PURCHASES:CREATE")]
    public async Task<IActionResult> Create(VendorBillCreateDto dto)
    {
        var result = await service.CreateAsync(dto);
        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error, code = result.ErrorCode });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "PURCHASES:UPDATE")]
    public async Task<IActionResult> Update(Guid id, VendorBillUpdateDto dto)
    {
        var result = await service.UpdateAsync(id, dto);
        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error, code = result.ErrorCode });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "PURCHASES:DELETE")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await service.DeleteAsync(id);
        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error, code = result.ErrorCode });
    }
}
