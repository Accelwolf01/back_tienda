using Microsoft.AspNetCore.Mvc;

namespace back_tienda.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { message = "API funcionando" });
    }
}
