using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("api/automation")]
public class AutomationController : ControllerBase
{
    private readonly IAutomationService _automationService;

    public AutomationController(IAutomationService automationService)
    {
        _automationService = automationService;
    }

    //estado atual da automação
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var status = await _automationService.GetStatusAsync();
        return Ok(status);
    }

    [HttpPost("enable")]
    public async Task<IActionResult> EnableAutomatic()
    {
        var result = await _automationService.EnableAutomaticAsync();
        return Ok(result);
    }

    [HttpPost("disable")]
    public async Task<IActionResult> DisableAutomatic()
    {
        var result = await _automationService.DisableAutomaticAsync();
        return Ok(result);
    }
    
    //Atualizar Metas ou Parâmetros
    [HttpPut("targets")]
    public async Task<IActionResult> UpdateTargets([FromBody] AutomationStatusDto targets)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var updated = await _automationService.UpdateTargetsAsync(targets);
        return Ok(updated);
    }

    //Forçar Avaliação das Regras
    [HttpPost("evaluate")]
    public async Task<IActionResult> EvaluateNow()
    {
        await _automationService.EvaluateRulesAsync();
        return Ok(new { Message = "Regras avaliadas" });
    }
}
