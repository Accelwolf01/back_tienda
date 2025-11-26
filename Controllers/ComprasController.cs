using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using back_tienda.Core.DTOs;
using back_tienda.Core.Interfaces;
using System.Security.Claims;

namespace back_tienda.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ComprasController : ControllerBase
{
    private readonly ICompraService _compraService;

    public ComprasController(ICompraService compraService)
    {
        _compraService = compraService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CompraDto>>> GetAll()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var idTiendaClaim = User.Claims.FirstOrDefault(c => c.Type == "IdTienda")?.Value;
        if (string.IsNullOrEmpty(idTiendaClaim))
        {
            return BadRequest("No se pudo obtener la tienda del usuario");
        }

        var idTienda = Guid.Parse(idTiendaClaim);
        var compras = await _compraService.GetByTiendaAsync(idTienda);
        return Ok(compras);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CompraDto>> GetById(Guid id)
    {
        var compra = await _compraService.GetByIdAsync(id);
        if (compra == null) return NotFound();
        return Ok(compra);
    }

    [HttpGet("tienda/{idTienda}")]
    public async Task<ActionResult<IEnumerable<CompraDto>>> GetByTienda(Guid idTienda)
    {
        var compras = await _compraService.GetByTiendaAsync(idTienda);
        return Ok(compras);
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN_SISTEMA,DUEÑO_TIENDA,EMPLEADO")]
    public async Task<ActionResult<CompraDto>> Create(CrearCompraDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var tiendaIdClaim = User.FindFirst("IdTienda")?.Value;
        if (!dto.IdTienda.HasValue && !string.IsNullOrEmpty(tiendaIdClaim))
        {
            if (Guid.TryParse(tiendaIdClaim, out var tiendaGuid))
            {
                dto = dto with { IdTienda = tiendaGuid };
            }
        }

        var compra = await _compraService.CrearAsync(Guid.Parse(userId), dto);
        return CreatedAtAction(nameof(GetById), new { id = compra.IdCompra }, compra);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN_SISTEMA,DUEÑO_TIENDA,EMPLEADO")]
    public async Task<ActionResult<CompraDto>> Update(Guid id, CrearCompraDto dto)
    {
        var compra = await _compraService.ActualizarAsync(id, dto);
        return Ok(compra);
    }
}
