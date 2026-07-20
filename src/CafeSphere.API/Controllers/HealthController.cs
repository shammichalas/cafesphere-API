using Microsoft.AspNetCore.Mvc;

namespace CafeSphere.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Ping()
    {
        return Ok(new
        {
            status = "Healthy",
            timestamp = DateTime.UtcNow,
            service = "CafeSphere API Backend"
        });
    }
}
