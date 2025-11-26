using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using back_tienda.Core.DTOs;
using back_tienda.Core.Interfaces;

namespace back_tienda.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly IProductoService _productoService;

    public ProductosController(IProductoService productoService)
    {
        _productoService = productoService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductoDto>>> GetAll()
    {
        var idTiendaClaim = User.Claims.FirstOrDefault(c => c.Type == "IdTienda")?.Value;
        if (string.IsNullOrEmpty(idTiendaClaim))
        {
            return BadRequest("No se pudo obtener la tienda del usuario");
        }

        var idTienda = Guid.Parse(idTiendaClaim);
        var productos = await _productoService.GetByTiendaAsync(idTienda);
        return Ok(productos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductoDto>> GetById(Guid id)
    {
        var producto = await _productoService.GetByIdAsync(id);
        if (producto == null) return NotFound();
        return Ok(producto);
    }

    [HttpGet("tienda/{idTienda}")]
    public async Task<ActionResult<IEnumerable<ProductoDto>>> GetByTienda(Guid idTienda)
    {
        var productos = await _productoService.GetByTiendaAsync(idTienda);
        return Ok(productos);
    }

    [HttpGet("tienda/{idTienda}/buscar")]
    public async Task<ActionResult<IEnumerable<ProductoDto>>> Buscar(Guid idTienda, [FromQuery] string termino)
    {
        var productos = await _productoService.BuscarAsync(idTienda, termino);
        return Ok(productos);
    }

    [HttpGet("tienda/{idTienda}/stock-bajo")]
    public async Task<ActionResult<IEnumerable<ProductoDto>>> GetStockBajo(Guid idTienda)
    {
        var productos = await _productoService.GetStockBajoAsync(idTienda);
        return Ok(productos);
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN_SISTEMA,DUEÑO_TIENDA,EMPLEADO")]
    public async Task<ActionResult<ProductoDto>> Create(CrearProductoDto dto)
    {
        var idTiendaClaim = User.Claims.FirstOrDefault(c => c.Type == "IdTienda")?.Value;
        
        // Si no viene en el DTO ni en el claim, error
        if (string.IsNullOrEmpty(idTiendaClaim) && !dto.IdTienda.HasValue)
        {
            return BadRequest(new { 
                message = "No se pudo obtener la tienda del usuario. Por favor, cierre sesión y vuelva a iniciar sesión.",
                debug = new {
                    claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList(),
                    dtoIdTienda = dto.IdTienda
                }
            });
        }

        // Usar el IdTienda del claim si no viene en el DTO
        var idTienda = dto.IdTienda ?? (string.IsNullOrEmpty(idTiendaClaim) ? Guid.Empty : Guid.Parse(idTiendaClaim));
        var dtoConTienda = dto with { IdTienda = idTienda };
        
        var producto = await _productoService.CrearAsync(dtoConTienda);
        return CreatedAtAction(nameof(GetById), new { id = producto.IdProducto }, producto);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN_SISTEMA,DUEÑO_TIENDA,EMPLEADO")]
    public async Task<ActionResult<ProductoDto>> Update(Guid id, ActualizarProductoDto dto)
    {
        var producto = await _productoService.ActualizarAsync(id, dto);
        return Ok(producto);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "ADMIN_SISTEMA,DUEÑO_TIENDA")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _productoService.EliminarAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
}
