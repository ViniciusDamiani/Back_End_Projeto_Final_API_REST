using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

[ApiController]
[Route("api/devices/{deviceId:guid}/measurements")]
public class MeasurementsController : ControllerBase
{
    private readonly IMeasurementService _measurementService;

    public MeasurementsController(IMeasurementService measurementService)
    {
        _measurementService = measurementService;
    }

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatest([FromRoute] Guid deviceId)
    {
        if (deviceId == Guid.Empty)
        {
            return BadRequest("deviceId inválido.");
        }
        var result = await _measurementService.GetLatestByDeviceAsync(deviceId);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromRoute] Guid deviceId, [FromBody] MeasurementCreateDto dto)
    {
        if (deviceId == Guid.Empty)
        {
            return BadRequest("deviceId inválido.");
        }
        var created = await _measurementService.CreateAsync(deviceId, dto);
        return CreatedAtAction(nameof(GetLatest), new { deviceId = created.DeviceId }, created);
    }
}
