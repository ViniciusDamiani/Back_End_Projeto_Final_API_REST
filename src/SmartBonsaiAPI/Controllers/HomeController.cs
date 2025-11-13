using Microsoft.AspNetCore.Mvc;

namespace SmartBonsaiAPI.Controllers
{
    [ApiController]
    [Route("/")]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public IActionResult Index()
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
    }
}
