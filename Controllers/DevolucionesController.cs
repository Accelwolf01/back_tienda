using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using back_tienda.Core.DTOs;
using back_tienda.Core.Interfaces;
using System.Security.Claims;

namespace back_tienda.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DevolucionesController : ControllerBase
{
    private readonly IDevolucionService _devolucionService;

    public DevolucionesController(IDevolucionService devolucionService)
    {
        _devolucionService = devolucionService;
    }

    [HttpGet("venta/{idVenta}")]
    public async Task<ActionResult<IEnumerable<DevolucionDto>>> GetByVenta(Guid idVenta)
    {
        var devoluciones = await _devolucionService.GetByVentaAsync(idVenta);
        return Ok(devoluciones);
    }

    [HttpPost]
    public async Task<ActionResult<DevolucionDto>> Create(CrearDevolucionDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var devolucion = await _devolucionService.CrearAsync(Guid.Parse(userId), dto);
        return Ok(devolucion);
    }
}
