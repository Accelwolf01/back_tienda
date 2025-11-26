using Microsoft.EntityFrameworkCore.Storage;
using back_tienda.Core.Interfaces;
using back_tienda.Infrastructure.Data;

namespace back_tienda.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        
        // Inicializar repositorios
        Usuarios = new UsuarioRepository(_context);
        Tiendas = new TiendaRepository(_context);
        EmpleadoTiendas = new EmpleadoTiendaRepository(_context);
        Productos = new ProductoRepository(_context);
        Categorias = new CategoriaRepository(_context);
        Proveedores = new ProveedorRepository(_context);
        Ventas = new VentaRepository(_context);
        Compras = new CompraRepository(_context);
        Devoluciones = new DevolucionRepository(_context);
        Mermas = new MermaRepository(_context);
    }

    public IUsuarioRepository Usuarios { get; private set; }
    public ITiendaRepository Tiendas { get; private set; }
    public IEmpleadoTiendaRepository EmpleadoTiendas { get; private set; }
    public IProductoRepository Productos { get; private set; }
    public ICategoriaRepository Categorias { get; private set; }
    public IProveedorRepository Proveedores { get; private set; }
    public IVentaRepository Ventas { get; private set; }
    public ICompraRepository Compras { get; private set; }
    public IDevolucionRepository Devoluciones { get; private set; }
    public IMermaRepository Mermas { get; private set; }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await SaveChangesAsync();
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
            }
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
