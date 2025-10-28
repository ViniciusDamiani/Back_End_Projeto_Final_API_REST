using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

[ApiController]
[Route("api/measurements")]
public class MeasurementsController : ControllerBase
{
    private readonly IMeasurementService _measurementService;

    public MeasurementsController(IMeasurementService measurementService)
    {
        _measurementService = measurementService;
    }

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatest([FromQuery] Guid deviceId)
    {
        var result = await _measurementService.GetLatestByDeviceAsync(deviceId);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MeasurementCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var created = await _measurementService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetLatest), new { deviceId = created.DeviceId }, created);
    }
}
