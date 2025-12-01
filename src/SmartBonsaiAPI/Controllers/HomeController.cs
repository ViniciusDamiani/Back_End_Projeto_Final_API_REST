using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace SmartBonsaiAPI.Controllers
{
    [ApiController]
    [Route("api/info")]
    public class HomeController : ControllerBase
    {
        private readonly IEmailSender _emailSender;

        public HomeController(IEmailSender emailSender = null)
        {
            _emailSender = emailSender;
        }

        [HttpGet]
        public IActionResult ApiInfo()
        {
            return Ok(new
            {
                message = "SmartBonsai API está em execução!",
                status = "Online",
                endpoints = new[]
                {
                    "/api/actuators",
                    "/api/measurements",
                    "/api/automation"
                }
            });
        }

        [HttpGet("config")]
        public IActionResult GetConfig()
        {
            return Ok(new
            {
                weatherApiKey = Environment.GetEnvironmentVariable("WEATHER_API_KEY") ?? string.Empty,
                weatherCity = Environment.GetEnvironmentVariable("WEATHER_CITY") ?? "Criciúma,BR"
            });
        }

        [HttpPost("test-email")]
        public async Task<IActionResult> TestEmail()
        {
            if (_emailSender == null)
            {
                return BadRequest(new { error = "Serviço de email não configurado" });
            }

            try
            {
                await _emailSender.SendAsync(
                    "SmartBonsai - Teste de Email",
                    "Este é um email de teste da API SmartBonsai.\n\nSe você recebeu esta mensagem, o envio de email está funcionando corretamente!"
                );
                return Ok(new { message = "Email de teste enviado com sucesso!" });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { error = $"Erro ao enviar email: {ex.Message}" });
            }
        }
    }
}
