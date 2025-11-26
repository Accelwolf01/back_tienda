using Microsoft.EntityFrameworkCore;
using back_tienda.Core.Entities;
using back_tienda.Core.Enums;
using back_tienda.Core.Interfaces;
using back_tienda.Infrastructure.Data;

namespace back_tienda.Infrastructure.Repositories;

public class VentaRepository : GenericRepository<Venta>, IVentaRepository
{
    public VentaRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Venta>> GetByTiendaAsync(Guid idTienda)
    {
        return await _dbSet
            .Where(v => v.IdTienda == idTienda)
            .Include(v => v.Usuario)
            .Include(v => v.DetalleVentas)
                .ThenInclude(d => d.Producto)
            .OrderByDescending(v => v.FechaVenta)
            .ToListAsync();
    }

    public async Task<IEnumerable<Venta>> GetByFechaRangoAsync(Guid idTienda, DateTime inicio, DateTime fin)
    {
        return await _dbSet
            .Where(v => v.IdTienda == idTienda && 
                       v.FechaVenta >= inicio && 
                       v.FechaVenta <= fin &&
                       v.Estado == EstadoDocumento.ACTIVO)
            .Include(v => v.Usuario)
            .Include(v => v.DetalleVentas)
                .ThenInclude(d => d.Producto)
            .OrderByDescending(v => v.FechaVenta)
            .ToListAsync();
    }

    public async Task<Venta?> GetWithDetallesAsync(Guid id)
    {
        return await _dbSet
            .Include(v => v.Usuario)
            .Include(v => v.Tienda)
            .Include(v => v.DetalleVentas)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(v => v.IdVenta == id);
    }

    public async Task<decimal> GetTotalVentasDiaAsync(Guid idTienda, DateTime fecha)
    {
        return await _dbSet
            .Where(v => v.IdTienda == idTienda && 
                       v.FechaVenta.Date == fecha.Date &&
                       v.Estado == EstadoDocumento.ACTIVO)
            .SumAsync(v => v.TotalVenta);
    }

    public async Task<decimal> GetTotalVentasMesAsync(Guid idTienda, int año, int mes)
    {
        return await _dbSet
            .Where(v => v.IdTienda == idTienda && 
                       v.FechaVenta.Year == año &&
                       v.FechaVenta.Month == mes &&
                       v.Estado == EstadoDocumento.ACTIVO)
            .SumAsync(v => v.TotalVenta);
    }
}

public class CompraRepository : GenericRepository<Compra>, ICompraRepository
{
    public CompraRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Compra>> GetByTiendaAsync(Guid idTienda)
    {
        return await _dbSet
            .Where(c => c.IdTienda == idTienda)
            .Include(c => c.Usuario)
            .Include(c => c.Proveedor)
            .Include(c => c.DetalleCompras)
                .ThenInclude(d => d.Producto)
            .OrderByDescending(c => c.FechaCompra)
            .ToListAsync();
    }

    public async Task<IEnumerable<Compra>> GetByProveedorAsync(Guid idProveedor)
    {
        return await _dbSet
            .Where(c => c.IdProveedor == idProveedor)
            .Include(c => c.Usuario)
            .Include(c => c.DetalleCompras)
                .ThenInclude(d => d.Producto)
            .OrderByDescending(c => c.FechaCompra)
            .ToListAsync();
    }

    public async Task<Compra?> GetWithDetallesAsync(Guid id)
    {
        return await _dbSet
            .Include(c => c.Usuario)
            .Include(c => c.Tienda)
            .Include(c => c.Proveedor)
            .Include(c => c.DetalleCompras)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(c => c.IdCompra == id);
    }

    public async Task<bool> PuedeEditarAsync(Guid idCompra)
    {
        var compra = await _dbSet.FindAsync(idCompra);
        if (compra == null) return false;
        
        return compra.PuedeEditar && 
               compra.FechaLimiteEdicion.HasValue && 
               DateTime.UtcNow <= compra.FechaLimiteEdicion.Value;
    }
}

public class DevolucionRepository : GenericRepository<Devolucion>, IDevolucionRepository
{
    public DevolucionRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Devolucion>> GetByVentaAsync(Guid idVenta)
    {
        return await _dbSet
            .Where(d => d.IdVenta == idVenta)
            .Include(d => d.Producto)
            .Include(d => d.Usuario)
            .OrderByDescending(d => d.FechaDevolucion)
            .ToListAsync();
    }

    public async Task<int> GetCantidadDevueltaAsync(Guid idVenta, Guid idProducto)
    {
        return await _dbSet
            .Where(d => d.IdVenta == idVenta && d.IdProducto == idProducto)
            .SumAsync(d => d.Cantidad);
    }
}

public class MermaRepository : GenericRepository<Merma>, IMermaRepository
{
    public MermaRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Merma>> GetByTiendaAsync(Guid idTienda)
    {
        return await _dbSet
            .Where(m => m.IdTienda == idTienda)
            .Include(m => m.Producto)
            .Include(m => m.Usuario)
            .OrderByDescending(m => m.FechaMerma)
            .ToListAsync();
    }

    public async Task<IEnumerable<Merma>> GetByProductoAsync(Guid idProducto)
    {
        return await _dbSet
            .Where(m => m.IdProducto == idProducto)
            .Include(m => m.Usuario)
            .OrderByDescending(m => m.FechaMerma)
            .ToListAsync();
    }
}
