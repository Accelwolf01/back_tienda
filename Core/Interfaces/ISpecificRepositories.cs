using back_tienda.Core.Entities;
using back_tienda.Core.Enums;

namespace back_tienda.Core.Interfaces;

public interface IUsuarioRepository : IGenericRepository<Usuario>
{
    Task<Usuario?> GetByCorreoAsync(string correo);
    Task<Usuario?> GetByUsuarioAsync(string usuario);
    Task<bool> ExisteCorreoAsync(string correo);
    Task<bool> ExisteUsuarioAsync(string usuario);
    Task<IEnumerable<Usuario>> GetByRolAsync(TipoRol rol);
}

public interface ITiendaRepository : IGenericRepository<Tienda>
{
    Task<IEnumerable<Tienda>> GetByDueñoAsync(Guid idDueño);
    Task<Tienda?> GetWithProductosAsync(Guid id);
    Task<Tienda?> GetWithCategoriasAsync(Guid id);
}

public interface IEmpleadoTiendaRepository : IGenericRepository<EmpleadoTienda>
{
    Task<IEnumerable<EmpleadoTienda>> GetByEmpleadoAsync(Guid idEmpleado);
    Task<IEnumerable<EmpleadoTienda>> GetByTiendaAsync(Guid idTienda);
}

public interface IProductoRepository : IGenericRepository<Producto>
{
    Task<IEnumerable<Producto>> GetByTiendaAsync(Guid idTienda);
    Task<IEnumerable<Producto>> BuscarProductosAsync(Guid idTienda, string termino);
    Task<IEnumerable<Producto>> GetProductosStockBajoAsync(Guid idTienda);
    Task<Producto?> GetByCodigoAsync(Guid idTienda, string codigo);
    Task<bool> ValidarStockDisponibleAsync(Guid idProducto, int cantidad);
}

public interface ICategoriaRepository : IGenericRepository<Categoria>
{
    Task<IEnumerable<Categoria>> GetByTiendaAsync(Guid idTienda);
    Task<bool> ExisteNombreEnTiendaAsync(Guid idTienda, string nombre);
}

public interface IProveedorRepository : IGenericRepository<Proveedor>
{
    Task<IEnumerable<Proveedor>> GetByTiendaAsync(Guid idTienda);
    Task<bool> ExisteNombreEnTiendaAsync(Guid idTienda, string nombre);
}

public interface IVentaRepository : IGenericRepository<Venta>
{
    Task<IEnumerable<Venta>> GetByTiendaAsync(Guid idTienda);
    Task<IEnumerable<Venta>> GetByFechaRangoAsync(Guid idTienda, DateTime inicio, DateTime fin);
    Task<Venta?> GetWithDetallesAsync(Guid id);
    Task<decimal> GetTotalVentasDiaAsync(Guid idTienda, DateTime fecha);
    Task<decimal> GetTotalVentasMesAsync(Guid idTienda, int año, int mes);
}

public interface ICompraRepository : IGenericRepository<Compra>
{
    Task<IEnumerable<Compra>> GetByTiendaAsync(Guid idTienda);
    Task<IEnumerable<Compra>> GetByProveedorAsync(Guid idProveedor);
    Task<Compra?> GetWithDetallesAsync(Guid id);
    Task<bool> PuedeEditarAsync(Guid idCompra);
}

public interface IDevolucionRepository : IGenericRepository<Devolucion>
{
    Task<IEnumerable<Devolucion>> GetByVentaAsync(Guid idVenta);
    Task<int> GetCantidadDevueltaAsync(Guid idVenta, Guid idProducto);
}

public interface IMermaRepository : IGenericRepository<Merma>
{
    Task<IEnumerable<Merma>> GetByTiendaAsync(Guid idTienda);
    Task<IEnumerable<Merma>> GetByProductoAsync(Guid idProducto);
}
