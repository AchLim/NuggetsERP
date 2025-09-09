using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nuggets.Application.Common.Services;
using Nuggets.Application.DTOs;

namespace Nuggets.API.Controllers;

[ApiController]
[Route("v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class SupplierController(ILogger<SupplierController> logger, ISupplierService service) : ControllerBase
{
    private readonly ILogger<SupplierController> _logger = logger;
    private readonly ISupplierService _service = service;

    [HttpGet]
    [Authorize(Policy = "SUPPLIERS:READ")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "SUPPLIERS:READ")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpPost]
    [Authorize(Policy = "SUPPLIERS:CREATE")]
    public async Task<IActionResult> Create(SupplierCreateDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "SUPPLIERS:UPDATE")]
    public async Task<IActionResult> Update(Guid id, SupplierUpdateDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "SUPPLIERS:DELETE")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id);
        return result.IsSuccess ? Ok() : NotFound(result.Error);
    }
}