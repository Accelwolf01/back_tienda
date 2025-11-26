using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using back_tienda.Core.DTOs;
using back_tienda.Core.Interfaces;
using System.Security.Claims;

namespace back_tienda.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MermasController : ControllerBase
{
    private readonly IMermaService _mermaService;

    public MermasController(IMermaService mermaService)
    {
        _mermaService = mermaService;
    }

    [HttpGet("tienda/{idTienda}")]
    public async Task<ActionResult<IEnumerable<MermaDto>>> GetByTienda(Guid idTienda)
    {
        var mermas = await _mermaService.GetByTiendaAsync(idTienda);
        return Ok(mermas);
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN_SISTEMA,DUEÑO_TIENDA,EMPLEADO")]
    public async Task<ActionResult<MermaDto>> Create(CrearMermaDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var merma = await _mermaService.CrearAsync(Guid.Parse(userId), dto);
        return Ok(merma);
    }
}
