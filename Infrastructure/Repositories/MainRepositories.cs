using Microsoft.EntityFrameworkCore;
using back_tienda.Core.Entities;
using back_tienda.Core.Enums;
using back_tienda.Core.Interfaces;
using back_tienda.Infrastructure.Data;

namespace back_tienda.Infrastructure.Repositories;

public class UsuarioRepository : GenericRepository<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Usuario?> GetByCorreoAsync(string correo)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Correo == correo);
    }

    public async Task<Usuario?> GetByUsuarioAsync(string usuario)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.UsuarioNombre == usuario);
    }

    public async Task<bool> ExisteCorreoAsync(string correo)
    {
        return await _dbSet.AnyAsync(u => u.Correo == correo);
    }

    public async Task<bool> ExisteUsuarioAsync(string usuario)
    {
        return await _dbSet.AnyAsync(u => u.UsuarioNombre == usuario);
    }

    public async Task<IEnumerable<Usuario>> GetByRolAsync(TipoRol rol)
    {
        return await _dbSet.Where(u => u.Rol == rol).ToListAsync();
    }
}

public class TiendaRepository : GenericRepository<Tienda>, ITiendaRepository
{
    public TiendaRepository(ApplicationDbContext context) : base(context) { }

    public override async Task<IEnumerable<Tienda>> GetAllAsync()
    {
        return await _dbSet
            .Include(t => t.Dueño)
            .ToListAsync();
    }

    public override async Task<Tienda?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(t => t.Dueño)
            .FirstOrDefaultAsync(t => t.IdTienda == id);
    }

    public async Task<IEnumerable<Tienda>> GetByDueñoAsync(Guid idDueño)
    {
        return await _dbSet
            .Where(t => t.IdDueño == idDueño)
            .Include(t => t.Dueño)
            .ToListAsync();
    }

    public async Task<Tienda?> GetWithProductosAsync(Guid id)
    {
        return await _dbSet
            .Include(t => t.Productos)
            .FirstOrDefaultAsync(t => t.IdTienda == id);
    }

    public async Task<Tienda?> GetWithCategoriasAsync(Guid id)
    {
        return await _dbSet
            .Include(t => t.Categorias)
            .FirstOrDefaultAsync(t => t.IdTienda == id);
    }
}

public class EmpleadoTiendaRepository : GenericRepository<EmpleadoTienda>, IEmpleadoTiendaRepository
{
    public EmpleadoTiendaRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<EmpleadoTienda>> GetByEmpleadoAsync(Guid idEmpleado)
    {
        return await _dbSet
            .Where(et => et.IdEmpleado == idEmpleado)
            .Include(et => et.Tienda)
            .ToListAsync();
    }

    public async Task<IEnumerable<EmpleadoTienda>> GetByTiendaAsync(Guid idTienda)
    {
        return await _dbSet
            .Where(et => et.IdTienda == idTienda)
            .Include(et => et.Empleado)
            .ToListAsync();
    }
}

public class ProductoRepository : GenericRepository<Producto>, IProductoRepository
{
    public ProductoRepository(ApplicationDbContext context) : base(context) { }

    public override async Task<IEnumerable<Producto>> GetAllAsync()
    {
        return await _dbSet
            .Where(p => p.Estado == EstadoUsuario.ACTIVO)
            .Include(p => p.Categoria)
            .OrderBy(p => p.NombreProducto)
            .ToListAsync();
    }

    public async Task<IEnumerable<Producto>> GetByTiendaAsync(Guid idTienda)
    {
        return await _dbSet
            .Where(p => p.IdTienda == idTienda && p.Estado == EstadoUsuario.ACTIVO)
            .Include(p => p.Categoria)
            .OrderBy(p => p.NombreProducto)
            .ToListAsync();
    }

    public async Task<IEnumerable<Producto>> BuscarProductosAsync(Guid idTienda, string termino)
    {
        return await _dbSet
            .Where(p => p.IdTienda == idTienda && 
                       p.Estado == EstadoUsuario.ACTIVO &&
                       (p.NombreProducto.Contains(termino) || 
                        (p.CodigoProducto != null && p.CodigoProducto.Contains(termino))))
            .Include(p => p.Categoria)
            .OrderBy(p => p.NombreProducto)
            .ToListAsync();
    }

    public async Task<IEnumerable<Producto>> GetProductosStockBajoAsync(Guid idTienda)
    {
        return await _dbSet
            .Where(p => p.IdTienda == idTienda && 
                       p.Estado == EstadoUsuario.ACTIVO &&
                       p.StockActual <= p.StockMinimo)
            .Include(p => p.Categoria)
            .OrderBy(p => p.StockActual)
            .ToListAsync();
    }

    public async Task<Producto?> GetByCodigoAsync(Guid idTienda, string codigo)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.IdTienda == idTienda && p.CodigoProducto == codigo);
    }

    public async Task<bool> ValidarStockDisponibleAsync(Guid idProducto, int cantidad)
    {
        var producto = await _dbSet.FindAsync(idProducto);
        return producto != null && producto.StockActual >= cantidad;
    }
}

public class CategoriaRepository : GenericRepository<Categoria>, ICategoriaRepository
{
    public CategoriaRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Categoria>> GetByTiendaAsync(Guid idTienda)
    {
        return await _dbSet
            .Where(c => c.IdTienda == idTienda)
            .OrderBy(c => c.NombreCategoria)
            .ToListAsync();
    }

    public async Task<bool> ExisteNombreEnTiendaAsync(Guid idTienda, string nombre)
    {
        return await _dbSet.AnyAsync(c => c.IdTienda == idTienda && c.NombreCategoria == nombre);
    }
}

public class ProveedorRepository : GenericRepository<Proveedor>, IProveedorRepository
{
    public ProveedorRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Proveedor>> GetByTiendaAsync(Guid idTienda)
    {
        return await _dbSet
            .Where(p => p.IdTienda == idTienda && p.Estado == EstadoUsuario.ACTIVO)
            .OrderBy(p => p.NombreProveedor)
            .ToListAsync();
    }

    public async Task<bool> ExisteNombreEnTiendaAsync(Guid idTienda, string nombre)
    {
        return await _dbSet.AnyAsync(p => p.IdTienda == idTienda && p.NombreProveedor == nombre);
    }
}
