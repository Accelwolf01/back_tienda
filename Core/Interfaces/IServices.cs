using back_tienda.Core.DTOs;

namespace back_tienda.Core.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<UsuarioDto> RegistrarUsuarioAsync(RegistroUsuarioDto request);
    Task<bool> CambiarContraseñaAsync(Guid idUsuario, CambiarContraseñaDto request);
    Task<string> GenerarTokenRecuperacionAsync(string correo);
    Task<bool> RecuperarContraseñaAsync(string token, string nuevaContraseña);
}

public interface IUsuarioService
{
    Task<UsuarioDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<UsuarioDto>> GetAllAsync();
    Task<UsuarioDto> ActualizarAsync(Guid id, ActualizarUsuarioDto dto);
    Task<bool> EliminarAsync(Guid id);
}

public interface ITiendaService
{
    Task<TiendaDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<TiendaDto>> GetAllAsync();
    Task<IEnumerable<TiendaDto>> GetByDueñoAsync(Guid idDueño);
    Task<TiendaDto> CrearAsync(CrearTiendaDto dto);
    Task<TiendaDto> ActualizarAsync(Guid id, ActualizarTiendaDto dto);
    Task<bool> EliminarAsync(Guid id);
}

public interface IProductoService
{
    Task<IEnumerable<ProductoDto>> GetAllAsync();
    Task<ProductoDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ProductoDto>> GetByTiendaAsync(Guid idTienda);
    Task<IEnumerable<ProductoDto>> BuscarAsync(Guid idTienda, string termino);
    Task<IEnumerable<ProductoDto>> GetStockBajoAsync(Guid idTienda);
    Task<ProductoDto> CrearAsync(CrearProductoDto dto);
    Task<ProductoDto> ActualizarAsync(Guid id, ActualizarProductoDto dto);
    Task<bool> EliminarAsync(Guid id);
}

public interface ICategoriaService
{
    Task<IEnumerable<CategoriaDto>> GetByTiendaAsync(Guid idTienda);
    Task<CategoriaDto> CrearAsync(CrearCategoriaDto dto);
    Task<bool> EliminarAsync(Guid id);
}

public interface IProveedorService
{
    Task<IEnumerable<ProveedorDto>> GetByTiendaAsync(Guid idTienda);
    Task<ProveedorDto> CrearAsync(CrearProveedorDto dto);
    Task<bool> EliminarAsync(Guid id);
}

public interface IVentaService
{
    Task<IEnumerable<VentaDto>> GetAllAsync();
    Task<VentaDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<VentaDto>> GetByTiendaAsync(Guid idTienda);
    Task<VentaDto> CrearAsync(Guid idUsuario, CrearVentaDto dto);
    Task<bool> CancelarAsync(Guid id, Guid idUsuario, string motivo);
    Task<VentaDto> ActualizarAsync(Guid id, Guid idUsuario, ActualizarVentaDto dto);
}

public interface ICompraService
{
    Task<IEnumerable<CompraDto>> GetAllAsync();
    Task<CompraDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<CompraDto>> GetByTiendaAsync(Guid idTienda);
    Task<CompraDto> CrearAsync(Guid idUsuario, CrearCompraDto dto);
    Task<CompraDto> ActualizarAsync(Guid id, CrearCompraDto dto);
}

public interface IDevolucionService
{
    Task<DevolucionDto> CrearAsync(Guid idUsuario, CrearDevolucionDto dto);
    Task<IEnumerable<DevolucionDto>> GetByVentaAsync(Guid idVenta);
}

public interface IMermaService
{
    Task<MermaDto> CrearAsync(Guid idUsuario, CrearMermaDto dto);
    Task<IEnumerable<MermaDto>> GetByTiendaAsync(Guid idTienda);
}

public interface IReporteService
{
    Task<DashboardDto> GetDashboardAsync(Guid idTienda);
    Task<ReporteDto> GetResumenAsync(Guid idTienda);
    Task<ReporteVentasDto> GetReporteVentasAsync(Guid idTienda, DateTime inicio, DateTime fin);
    Task<IEnumerable<AlertaStockDto>> GetAlertasStockAsync(Guid idTienda);
}
