using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nuggets.Application.Common.Services;
using Nuggets.Application.DTOs;

namespace Nuggets.API.Controllers;

[ApiController]
[Route("v{version:apiVersion}/expense")]
[ApiVersion("1.0")]
public class ExpenseController(IExpenseService service) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "EXPENSES:READ")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10
    )
    {
        var result = await service.GetPagedAsync(page, pageSize);
        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error, code = result.ErrorCode });
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "EXPENSES:READ")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await service.GetByIdAsync(id);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpPost]
    [Authorize(Policy = "EXPENSES:CREATE")]
    public async Task<IActionResult> Create(ExpenseCreateDto dto)
    {
        var result = await service.CreateAsync(dto);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "EXPENSES:UPDATE")]
    public async Task<IActionResult> Update(Guid id, ExpenseUpdateDto dto)
    {
        var result = await service.UpdateAsync(id, dto);

        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "EXPENSES:DELETE")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await service.DeleteAsync(id);

        return result.IsSuccess ? Ok() : NotFound(result.Error);
    }
}