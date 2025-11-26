using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using back_tienda.Core.DTOs;
using back_tienda.Core.Interfaces;

namespace back_tienda.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [HttpGet]
    [Authorize(Roles = "ADMIN_SISTEMA")]
    public async Task<ActionResult<IEnumerable<UsuarioDto>>> GetAll()
    {
        var usuarios = await _usuarioService.GetAllAsync();
        return Ok(usuarios);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UsuarioDto>> GetById(Guid id)
    {
        var usuario = await _usuarioService.GetByIdAsync(id);
        if (usuario == null) return NotFound();
        return Ok(usuario);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UsuarioDto>> Update(Guid id, ActualizarUsuarioDto dto)
    {
        var usuario = await _usuarioService.ActualizarAsync(id, dto);
        return Ok(usuario);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "ADMIN_SISTEMA")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _usuarioService.EliminarAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
}
