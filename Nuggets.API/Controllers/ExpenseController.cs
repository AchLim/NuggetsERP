using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nuggets.Application.Common.Services;
using Nuggets.Application.DTOs;

namespace Nuggets.API.Controllers;

[ApiController]
[Route("v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class ExpenseController(IExpenseService service) : ControllerBase
{
    private readonly IExpenseService _service = service;

    [HttpGet]
    [Authorize(Policy = "EXPENSES:READ")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "EXPENSES:READ")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpPost]
    [Authorize(Policy = "EXPENSES:CREATE")]
    public async Task<IActionResult> Create(ExpenseCreateDto dto)
    {
        var result = await _service.CreateAsync(dto);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "EXPENSES:UPDATE")]
    public async Task<IActionResult> Update(Guid id, ExpenseUpdateDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);

        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "EXPENSES:DELETE")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id);

        return result.IsSuccess ? Ok() : NotFound(result.Error);
    }
}