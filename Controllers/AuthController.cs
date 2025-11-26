using Microsoft.AspNetCore.Mvc;
using back_tienda.Core.DTOs;
using back_tienda.Core.Interfaces;

namespace back_tienda.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        var response = await _authService.LoginAsync(request);
        return Ok(response);
    }

    [HttpPost("registro")]
    public async Task<ActionResult<UsuarioDto>> Registro(RegistroUsuarioDto request)
    {
        try
        {
            var usuario = await _authService.RegistrarUsuarioAsync(request);
            return CreatedAtAction(nameof(Registro), new { id = usuario.IdUsuario }, usuario);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("cambiar-password")]
    public async Task<IActionResult> CambiarPassword(CambiarContraseñaDto request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _authService.CambiarContraseñaAsync(Guid.Parse(userId), request);
        if (!result) return BadRequest("No se pudo cambiar la contraseña");

        return Ok(new { message = "Contraseña actualizada correctamente" });
    }

    [HttpPost("recuperar-password/solicitar")]
    public async Task<IActionResult> SolicitarRecuperacion([FromBody] string correo)
    {
        var token = await _authService.GenerarTokenRecuperacionAsync(correo);
        return Ok(new { token }); 
    }

    [HttpPost("recuperar-password/confirmar")]
    public async Task<IActionResult> ConfirmarRecuperacion(string token, [FromBody] string nuevaPassword)
    {
        var result = await _authService.RecuperarContraseñaAsync(token, nuevaPassword);
        if (!result) return BadRequest("Token inválido o expirado");

        return Ok(new { message = "Contraseña restablecida correctamente" });
    }
}
