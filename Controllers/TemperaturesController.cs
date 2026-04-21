using Indigo_task.Models;
using Indigo_task.Services;
using Microsoft.AspNetCore.Mvc;

namespace Indigo_task.Controllers;

[ApiController]
[Route("api/temperatures")]
public sealed class TemperaturesController : ControllerBase
{
    private readonly ITemperatureService _temperatureService;

    public TemperaturesController(ITemperatureService temperatureService)
    {
        _temperatureService = temperatureService;
    }

    /// <summary>Returns min, max and average temperature for every city.</summary>
    [HttpGet]
    [ProducesResponseType<IEnumerable<CityStats>>(StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        return Ok(_temperatureService.GetAll().Values);
    }

    /// <summary>Returns min, max and average temperature for a single city.</summary>
    [HttpGet("{city}")]
    [ProducesResponseType<CityStats>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetByCity(string city)
    {
        var stats = _temperatureService.GetByCity(city);
        return stats is null ? NotFound($"City '{city}' not found.") : Ok(stats);
    }

    /// <summary>
    /// Returns cities where the average temperature is greater or smaller than the given value.
    /// </summary>
    /// <param name="comparison">Use 'gt' for greater than, 'lt' for less than.</param>
    /// <param name="value">The threshold temperature in Celsius.</param>
    [HttpGet("filter")]
    [ProducesResponseType<IEnumerable<CityStats>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Filter([FromQuery] string comparison, [FromQuery] double value)
    {
        var all = _temperatureService.GetAll().Values;

        var result = comparison.ToLowerInvariant() switch
        {
            "gt" => all.Where(s => s.Avg > value),
            "lt" => all.Where(s => s.Avg < value),
            _    => null
        };

        if (result is null)
            return BadRequest("Invalid 'comparison' value. Use 'gt' (greater than) or 'lt' (less than).");

        return Ok(result);
    }

    /// <summary>Reloads the data file and recalculates all temperature statistics.</summary>
    [HttpPost("recalculate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Recalculate()
    {
        _temperatureService.Recalculate();
        return Ok("Temperature statistics recalculated.");
    }
}
