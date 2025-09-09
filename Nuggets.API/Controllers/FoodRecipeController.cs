using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nuggets.Application.Common.Services;
using Nuggets.Application.DTOs;

namespace Nuggets.API.Controllers;

[ApiController]
[Route("v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class FoodRecipeController(ILogger<FoodRecipeController> logger, IFoodRecipeService service) : ControllerBase
{
    private readonly ILogger<FoodRecipeController> _logger = logger;
    private readonly IFoodRecipeService _service = service;

    [HttpGet]
    [Authorize(Policy = "FOOD_RECIPES:READ")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "FOOD_RECIPES:READ")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpPost]
    [Authorize(Policy = "FOOD_RECIPES:CREATE")]
    public async Task<IActionResult> Create(FoodRecipeCreateDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "FOOD_RECIPES:UPDATE")]
    public async Task<IActionResult> Update(Guid id, FoodRecipeUpdateDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "FOOD_RECIPES:DELETE")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id);
        return result.IsSuccess ? Ok() : NotFound(result.Error);
    }

    [HttpGet("product/{productId:guid}/cost")]
    public async Task<IActionResult> GetProductCost(Guid productId)
    {
        var totalCost = await _service.CalculateMaterialCostAsync(productId);
        return Ok(new { ProductId = productId, MaterialCost = totalCost });
    }
}