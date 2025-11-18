using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

[ApiController]
[Route("api/devices/{deviceId:int}/measurements")]
public class MeasurementsController : ControllerBase
{   
    private readonly IMeasurementService _measurementService; //lógica de negócio

    public MeasurementsController(IMeasurementService measurementService)
    {
        _measurementService = measurementService;
    }

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatest([FromRoute] int deviceId)
    {
        if (deviceId == 0)
        {
            return BadRequest("deviceId inválido.");
        }
        var result = await _measurementService.GetLatestByDeviceAsync(deviceId);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromRoute] int deviceId, [FromBody] MeasurementCreateDto dto)
    {
        if (deviceId == 0)
        {
            return BadRequest("deviceId inválido.");
        }
        var created = await _measurementService.CreateAsync(deviceId, dto);
        return CreatedAtAction(nameof(GetLatest), new { deviceId = created.DeviceId }, created); // retorna que foi criado e onde pode ser acessado, com o ID e o corpo 
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromRoute] int deviceId, [FromQuery] int limit = 100)
    {
        if (deviceId == 0)
        {
            return BadRequest("deviceId inválido.");
        }
        var history = await _measurementService.GetHistoryAsync(deviceId, limit);
        return Ok(history);
    }
}
