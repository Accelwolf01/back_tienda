using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using back_tienda.Core.DTOs;
using back_tienda.Core.Interfaces;
using System.Security.Claims;

namespace back_tienda.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class VentasController : ControllerBase
{
    private readonly IVentaService _ventaService;

    public VentasController(IVentaService ventaService)
    {
        _ventaService = ventaService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VentaDto>>> GetAll()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var idTiendaClaim = User.Claims.FirstOrDefault(c => c.Type == "IdTienda")?.Value;
        if (string.IsNullOrEmpty(idTiendaClaim))
        {
            return BadRequest("No se pudo obtener la tienda del usuario");
        }

        var idTienda = Guid.Parse(idTiendaClaim);
        var ventas = await _ventaService.GetByTiendaAsync(idTienda);
        return Ok(ventas);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VentaDto>> GetById(Guid id)
    {
        var venta = await _ventaService.GetByIdAsync(id);
        if (venta == null) return NotFound();
        return Ok(venta);
    }

    [HttpGet("tienda/{idTienda}")]
    public async Task<ActionResult<IEnumerable<VentaDto>>> GetByTienda(Guid idTienda)
    {
        var ventas = await _ventaService.GetByTiendaAsync(idTienda);
        return Ok(ventas);
    }

    [HttpPost]
    public async Task<ActionResult<VentaDto>> Create(CrearVentaDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var idTiendaClaim = User.Claims.FirstOrDefault(c => c.Type == "IdTienda")?.Value;
        if (string.IsNullOrEmpty(idTiendaClaim))
        {
            return BadRequest("No se pudo obtener la tienda del usuario");
        }

        var idTienda = Guid.Parse(idTiendaClaim);
        var dtoConTienda = dto with { IdTienda = idTienda };

        var venta = await _ventaService.CrearAsync(Guid.Parse(userId), dtoConTienda);
        return CreatedAtAction(nameof(GetById), new { id = venta.IdVenta }, venta);
    }

    [HttpPost("{id}/cancelar")]
    [Authorize(Roles = "ADMIN_SISTEMA,DUEÑO_TIENDA")]
    public async Task<IActionResult> Cancelar(Guid id, [FromBody] string motivo)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _ventaService.CancelarAsync(id, Guid.Parse(userId), motivo);
        if (!result) return BadRequest("No se pudo cancelar la venta");

        return Ok(new { message = "Venta cancelada correctamente" });
    }
}
