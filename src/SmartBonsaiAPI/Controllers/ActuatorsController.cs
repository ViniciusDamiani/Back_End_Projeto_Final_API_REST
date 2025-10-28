using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

[ApiController]
[Route("api/actuators")]
public class ActuatorsController : ControllerBase
{
    private readonly IActuatorService _actuatorService;

    public ActuatorsController(IActuatorService actuatorService)
    {
        _actuatorService = actuatorService;
    }

    [HttpPost("{id:guid}/commands")]
    public async Task<IActionResult> ExecuteCommand(Guid id, [FromBody] ActionCommandDto cmd)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var success = await _actuatorService.ExecuteCommandAsync(id, cmd);
        return success ? Ok(new { Message = "Comando executado com sucesso" }) : NotFound();
    }

    [HttpGet("{id:guid}/status")]
    public async Task<IActionResult> GetStatus(Guid id)
    {
        var status = await _actuatorService.GetStatusAsync(id);
        return status == null ? NotFound() : Ok(status);
    }
}
