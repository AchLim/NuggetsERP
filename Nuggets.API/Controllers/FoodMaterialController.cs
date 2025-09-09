using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nuggets.Application.Common.Services;
using Nuggets.Application.DTOs;

namespace Nuggets.API.Controllers;

[ApiController]
[Route("v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class FoodMaterialController(ILogger<FoodMaterialController> logger, IFoodMaterialService service) : ControllerBase
{
    private readonly ILogger<FoodMaterialController> _logger = logger;
    private readonly IFoodMaterialService _service = service;

    [HttpGet]
    [Authorize(Policy = "FOOD_MATERIALS:READ")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "FOOD_MATERIALS:READ")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpPost]
    [Authorize(Policy = "RequireAdmin")]
    [Authorize(Policy = "FOOD_MATERIALS:CREATE")]
    public async Task<IActionResult> Create(FoodMaterialCreateDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "FOOD_MATERIALS:UPDATE")]
    public async Task<IActionResult> Update(Guid id, FoodMaterialUpdateDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "FOOD_MATERIALS:DELETE")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id);
        return result.IsSuccess ? Ok() : NotFound(result.Error);
    }
}