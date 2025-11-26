using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using back_tienda.Core.DTOs;
using back_tienda.Core.Interfaces;

namespace back_tienda.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    private readonly ICategoriaService _categoriaService;

    public CategoriasController(ICategoriaService categoriaService)
    {
        _categoriaService = categoriaService;
    }

    [HttpGet("tienda/{idTienda}")]
    public async Task<ActionResult<IEnumerable<CategoriaDto>>> GetByTienda(Guid idTienda)
    {
        var categorias = await _categoriaService.GetByTiendaAsync(idTienda);
        return Ok(categorias);
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN_SISTEMA,DUEÑO_TIENDA,EMPLEADO")]
    public async Task<ActionResult<CategoriaDto>> Create(CrearCategoriaDto dto)
    {
        var categoria = await _categoriaService.CrearAsync(dto);
        return Ok(categoria);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "ADMIN_SISTEMA,DUEÑO_TIENDA")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _categoriaService.EliminarAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
}
