namespace back_tienda.Core.Interfaces;

public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}

public interface IUnitOfWork : IDisposable
{
    IUsuarioRepository Usuarios { get; }
    ITiendaRepository Tiendas { get; }
    IEmpleadoTiendaRepository EmpleadoTiendas { get; }
    IProductoRepository Productos { get; }
    ICategoriaRepository Categorias { get; }
    IProveedorRepository Proveedores { get; }
    IVentaRepository Ventas { get; }
    ICompraRepository Compras { get; }
    IDevolucionRepository Devoluciones { get; }
    IMermaRepository Mermas { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
