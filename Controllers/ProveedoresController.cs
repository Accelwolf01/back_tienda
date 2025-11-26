using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using back_tienda.Core.DTOs;
using back_tienda.Core.Interfaces;

namespace back_tienda.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProveedoresController : ControllerBase
{
    private readonly IProveedorService _proveedorService;

    public ProveedoresController(IProveedorService proveedorService)
    {
        _proveedorService = proveedorService;
    }

    [HttpGet("tienda/{idTienda}")]
    public async Task<ActionResult<IEnumerable<ProveedorDto>>> GetByTienda(Guid idTienda)
    {
        var proveedores = await _proveedorService.GetByTiendaAsync(idTienda);
        return Ok(proveedores);
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN_SISTEMA,DUEÑO_TIENDA,EMPLEADO")]
    public async Task<ActionResult<ProveedorDto>> Create(CrearProveedorDto dto)
    {
        var proveedor = await _proveedorService.CrearAsync(dto);
        return Ok(proveedor);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "ADMIN_SISTEMA,DUEÑO_TIENDA")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _proveedorService.EliminarAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
}
