using Microsoft.AspNetCore.Mvc;
using Nuggets.Application.Common.Services;
using Nuggets.Application.Services;

namespace Nuggets.API.Controllers;

[ApiController]
[Route("v{version:apiVersion}/uom")]
[ApiVersion("1.0")]
public class UomController(ILogger<UomController> logger, IUomService service) : ControllerBase
{
    /// <summary>
    /// Get all Units of Measure.
    /// </summary>
    [HttpGet]
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

    /// <summary>
    /// Get a Unit of Measure by Id.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var uom = await service.GetByIdAsync(id);
        if (uom == null) return NotFound();
        return Ok(uom);
    }

    /// <summary>
    /// Get a Unit of Measure by abbreviation (example: "kg").
    /// </summary>
    [HttpGet("abbr/{abbr}")]
    public async Task<IActionResult> GetByAbbreviation(string abbr, CancellationToken ct)
    {
        var uom = await service.GetByAbbreviationAsync(abbr);
        if (uom == null) return NotFound();
        return Ok(uom);
    }

    /// <summary>
    /// Convert a value between two UOMs using GUIDs.
    /// </summary>
    [HttpGet("convert/by-id")]
    public async Task<IActionResult> ConvertById(
        Guid fromUomId,
        Guid toUomId,
        decimal value,
        UomService.ConversionPurpose purpose = UomService.ConversionPurpose.Quantity,
        CancellationToken ct = default)
    {
        var result = await service.ConvertAsync(fromUomId, toUomId, value, purpose);
        if (result == null)
            return BadRequest("No conversion available between given units.");

        return Ok(new
        {
            FromUomId = fromUomId,
            ToUomId = toUomId,
            Input = value,
            Converted = result
        });
    }

    /// <summary>
    /// Convert a value between two UOMs using abbreviations.
    /// Example: /api/uom/convert/by-abbr?from=kg&to=g&value=2
    /// </summary>
    [HttpGet("convert/by-abbr")]
    public async Task<IActionResult> ConvertByAbbreviation(
        string from,
        string to,
        decimal value,
        UomService.ConversionPurpose purpose = UomService.ConversionPurpose.Quantity,
        CancellationToken ct = default)
    {
        var fromUom = await service.GetByAbbreviationAsync(from);
        var toUom = await service.GetByAbbreviationAsync(to);

        if (fromUom == null || toUom == null)
            return NotFound("One or both of the given UOM abbreviations were not found.");

        var result = await service.ConvertAsync(fromUom.Id, toUom.Id, value, purpose);
        if (result == null)
            return BadRequest("No conversion available between given units.");

        return Ok(new
        {
            From = fromUom.Abbreviation,
            To = toUom.Abbreviation,
            Input = value,
            Converted = result
        });
    }

    /// <summary>
    /// Get all conversions available for a given UOM.
    /// </summary>
    [HttpGet("{uomId:guid}/conversions")]
    public async Task<IActionResult> GetConversions(Guid uomId, CancellationToken ct)
    {
        var conversions = await service.GetConversionsAsync(uomId);
        return Ok(conversions.Select(c => new
        {
            c.Id,
            From = new { c.FromUom.Id, c.FromUom.Name, c.FromUom.Abbreviation },
            To = new { c.ToUom.Id, c.ToUom.Name, c.ToUom.Abbreviation },
            c.ConversionRate,
            c.IsBidirectional
        }));
    }
}