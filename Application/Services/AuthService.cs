using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using AutoMapper;
using back_tienda.Core.DTOs;
using back_tienda.Core.Entities;
using back_tienda.Core.Enums;
using back_tienda.Core.Interfaces;


namespace back_tienda.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;

    public AuthService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _configuration = configuration;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        // Buscar usuario
        var usuario = await _unitOfWork.Usuarios.GetByUsuarioAsync(request.Usuario);
        
        if (usuario == null)
        {
            throw new UnauthorizedAccessException("Usuario o contraseña incorrectos");
        }

        // Verificar si está bloqueado
        if (usuario.Estado == EstadoUsuario.BLOQUEADO && 
            usuario.BloqueadoHasta.HasValue && 
            usuario.BloqueadoHasta.Value > DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException($"Usuario bloqueado hasta {usuario.BloqueadoHasta.Value:dd/MM/yyyy HH:mm}");
        }

        // Verificar contraseña
        if (!BCrypt.Net.BCrypt.Verify(request.Contraseña, usuario.ContraseñaHash))
        {
            // Incrementar intentos fallidos
            usuario.IntentosFallidos++;
            
            if (usuario.IntentosFallidos >= 3)
            {
                usuario.Estado = EstadoUsuario.BLOQUEADO;
                usuario.BloqueadoHasta = DateTime.UtcNow.AddMinutes(30);
            }
            
            await _unitOfWork.Usuarios.UpdateAsync(usuario);
            await _unitOfWork.SaveChangesAsync();
            
            throw new UnauthorizedAccessException("Usuario o contraseña incorrectos");
        }

        // Resetear intentos fallidos
        if (usuario.IntentosFallidos > 0)
        {
            usuario.IntentosFallidos = 0;
            usuario.Estado = EstadoUsuario.ACTIVO;
            usuario.BloqueadoHasta = null;
            await _unitOfWork.Usuarios.UpdateAsync(usuario);
            await _unitOfWork.SaveChangesAsync();
        }

        // Generar token JWT
        var token = await GenerarTokenJWT(usuario);
        var expiracion = DateTime.UtcNow.AddHours(8);

        // Obtener IdTienda para la respuesta
        Guid? idTienda = null;
        if (usuario.Rol == TipoRol.DUEÑO_TIENDA)
        {
            var tiendas = await _unitOfWork.Tiendas.GetByDueñoAsync(usuario.IdUsuario);
            idTienda = tiendas.FirstOrDefault()?.IdTienda;
        }
        else if (usuario.Rol == TipoRol.EMPLEADO)
        {
            var empleadoTienda = await _unitOfWork.EmpleadoTiendas.GetByEmpleadoAsync(usuario.IdUsuario);
            idTienda = empleadoTienda.FirstOrDefault()?.IdTienda;
        }

        return new LoginResponseDto(
            usuario.IdUsuario,
            usuario.NombreCompleto,
            usuario.UsuarioNombre,
            usuario.Correo,
            usuario.Rol,
            token,
            expiracion,
            idTienda
        );
    }

    public async Task<UsuarioDto> RegistrarUsuarioAsync(RegistroUsuarioDto request)
    {
        // Verificar si el usuario ya existe
        if (await _unitOfWork.Usuarios.ExisteUsuarioAsync(request.Usuario))
        {
            throw new InvalidOperationException("El nombre de usuario ya está en uso");
        }

        if (await _unitOfWork.Usuarios.ExisteCorreoAsync(request.Correo))
        {
            throw new InvalidOperationException("El correo ya está registrado");
        }

        // Crear usuario
        var usuario = _mapper.Map<Usuario>(request);
        usuario.ContraseñaHash = BCrypt.Net.BCrypt.HashPassword(request.Contraseña);
        usuario.Estado = EstadoUsuario.ACTIVO;
        usuario.FechaCreacion = DateTime.UtcNow;

        await _unitOfWork.Usuarios.AddAsync(usuario);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<UsuarioDto>(usuario);
    }

    public async Task<bool> CambiarContraseñaAsync(Guid idUsuario, CambiarContraseñaDto request)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(idUsuario);
        
        if (usuario == null)
        {
            throw new KeyNotFoundException("Usuario no encontrado");
        }

        // Verificar contraseña actual
        if (!BCrypt.Net.BCrypt.Verify(request.ContraseñaActual, usuario.ContraseñaHash))
        {
            throw new UnauthorizedAccessException("La contraseña actual es incorrecta");
        }

        // Actualizar contraseña
        usuario.ContraseñaHash = BCrypt.Net.BCrypt.HashPassword(request.ContraseñaNueva);
        usuario.FechaUltimaModificacion = DateTime.UtcNow;

        await _unitOfWork.Usuarios.UpdateAsync(usuario);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<string> GenerarTokenRecuperacionAsync(string correo)
    {
        var usuario = await _unitOfWork.Usuarios.GetByCorreoAsync(correo);
        
        if (usuario == null)
        {
            // Por seguridad, no revelar si el correo existe
            return string.Empty;
        }

        // Generar token único
        var token = Guid.NewGuid().ToString("N");
        
        // Aquí deberías guardar el token en la tabla tokens_recuperacion
        // y enviar un correo al usuario
        
        return token;
    }

    public Task<bool> RecuperarContraseñaAsync(string token, string nuevaContraseña)
    {
        // Aquí deberías verificar el token en la tabla tokens_recuperacion
        // y actualizar la contraseña del usuario
        
        throw new NotImplementedException("Funcionalidad de recuperación de contraseña pendiente");
    }

    private async Task<string> GenerarTokenJWT(Usuario usuario)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey no configurada");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claimsList = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
            new Claim(ClaimTypes.Name, usuario.UsuarioNombre),
            new Claim(ClaimTypes.Email, usuario.Correo),
            new Claim(ClaimTypes.Role, usuario.Rol.ToString()),
            new Claim("NombreCompleto", usuario.NombreCompleto)
        };

        if (usuario.Rol == TipoRol.DUEÑO_TIENDA)
        {
            var tiendas = await _unitOfWork.Tiendas.GetByDueñoAsync(usuario.IdUsuario);
            var primeraTienda = tiendas.FirstOrDefault();
            if (primeraTienda != null)
            {
                claimsList.Add(new Claim("IdTienda", primeraTienda.IdTienda.ToString()));
            }
        }
        else if (usuario.Rol == TipoRol.EMPLEADO)
        {
            var empleadoTienda = await _unitOfWork.EmpleadoTiendas.GetByEmpleadoAsync(usuario.IdUsuario);
            var primeraTienda = empleadoTienda.FirstOrDefault();
            if (primeraTienda != null)
            {
                claimsList.Add(new Claim("IdTienda", primeraTienda.IdTienda.ToString()));
            }
        }

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claimsList,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
