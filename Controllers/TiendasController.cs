using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using back_tienda.Core.DTOs;
using back_tienda.Core.Interfaces;

namespace back_tienda.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TiendasController : ControllerBase
{
    private readonly ITiendaService _tiendaService;

    public TiendasController(ITiendaService tiendaService)
    {
        _tiendaService = tiendaService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TiendaDto>>> GetAll()
    {
        var tiendas = await _tiendaService.GetAllAsync();
        return Ok(tiendas);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TiendaDto>> GetById(Guid id)
    {
        var tienda = await _tiendaService.GetByIdAsync(id);
        if (tienda == null) return NotFound();
        return Ok(tienda);
    }

    [HttpGet("usuario/{idUsuario}")]
    public async Task<ActionResult<IEnumerable<TiendaDto>>> GetByUsuario(Guid idUsuario)
    {
        var tiendas = await _tiendaService.GetByDueñoAsync(idUsuario);
        return Ok(tiendas);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<TiendaDto>> Create(CrearTiendaDto dto)
    {
        var tienda = await _tiendaService.CrearAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = tienda.IdTienda }, tienda);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN_SISTEMA,DUEÑO_TIENDA")]
    public async Task<ActionResult<TiendaDto>> Update(Guid id, ActualizarTiendaDto dto)
    {
        var tienda = await _tiendaService.ActualizarAsync(id, dto);
        return Ok(tienda);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _tiendaService.EliminarAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
}
