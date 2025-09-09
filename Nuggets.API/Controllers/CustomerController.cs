using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nuggets.Application.Common.Services;
using Nuggets.Application.DTOs;

namespace Nuggets.API.Controllers;

[ApiController]
[Route("v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class CustomerController(ILogger<CustomerController> logger, ICustomerService service) : ControllerBase
{
    private readonly ILogger<CustomerController> _logger = logger;
    private readonly ICustomerService _service = service;
    
    [HttpGet]
    [Authorize(Policy = "CUSTOMERS:READ")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sort = null,
        [FromQuery] string? name = null,
        [FromQuery] string? email = null,
        [FromQuery] string? phone = null,
        [FromQuery] string? address = null
    )
    {
        var filters = new Dictionary<string, string?>
        {
            { "Name", name },
            { "Email", email },
            { "Phone", phone },
            { "Address", address }
        };
        var result = await _service.GetPagedAsync(page, pageSize, filters, sort);
        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "CUSTOMERS:READ")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpPost]
    [Authorize(Policy = "CUSTOMERS:CREATE")]
    public async Task<IActionResult> Create(CustomerCreateDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "CUSTOMERS:UPDATE")]
    public async Task<IActionResult> Update(Guid id, CustomerUpdateDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "CUSTOMERS:DELETE")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id);
        return result.IsSuccess ? Ok() : NotFound(result.Error);
    }
}