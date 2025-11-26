using back_tienda.Core.Enums;

namespace back_tienda.Core.Entities;

public class Tienda
{
    public Guid IdTienda { get; set; }
    public string NombreTienda { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? CorreoTienda { get; set; }
    public string? Nit { get; set; }
    public Guid IdDueño { get; set; }
    public EstadoUsuario Estado { get; set; } = EstadoUsuario.ACTIVO;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaUltimaModificacion { get; set; } = DateTime.UtcNow;

    // Navegación
    public virtual Usuario Dueño { get; set; } = null!;
    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
    public virtual ICollection<Categoria> Categorias { get; set; } = new List<Categoria>();
    public virtual ICollection<Proveedor> Proveedores { get; set; } = new List<Proveedor>();
    public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    public virtual ICollection<Compra> Compras { get; set; } = new List<Compra>();
}

public class EmpleadoTienda
{
    public Guid IdEmpleadoTienda { get; set; }
    public Guid IdEmpleado { get; set; }
    public Guid IdTienda { get; set; }
    public bool PuedeVerReportes { get; set; } = false;
    public bool PuedeGestionarInventario { get; set; } = true;
    public bool PuedeRegistrarVentas { get; set; } = true;
    public bool PuedeRegistrarCompras { get; set; } = false;
    public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;
    public EstadoUsuario Estado { get; set; } = EstadoUsuario.ACTIVO;

    public virtual Usuario Empleado { get; set; } = null!;
    public virtual Tienda Tienda { get; set; } = null!;
}

public class Categoria
{
    public Guid IdCategoria { get; set; }
    public Guid IdTienda { get; set; }
    public string NombreCategoria { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public virtual Tienda Tienda { get; set; } = null!;
    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}

public class Proveedor
{
    public Guid IdProveedor { get; set; }
    public Guid IdTienda { get; set; }
    public string NombreProveedor { get; set; } = string.Empty;
    public string? Nit { get; set; }
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
    public string? Direccion { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public EstadoUsuario Estado { get; set; } = EstadoUsuario.ACTIVO;

    public virtual Tienda Tienda { get; set; } = null!;
    public virtual ICollection<Compra> Compras { get; set; } = new List<Compra>();
}

public class Producto
{
    public Guid IdProducto { get; set; }
    public Guid IdTienda { get; set; }
    public Guid? IdCategoria { get; set; }
    public string? CodigoProducto { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal PrecioVenta { get; set; }
    public decimal? PrecioCompra { get; set; }
    public int StockActual { get; set; } = 0;
    public int StockMinimo { get; set; } = 0;
    public string UnidadMedida { get; set; } = "UNIDAD";
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaUltimaModificacion { get; set; } = DateTime.UtcNow;
    public EstadoUsuario Estado { get; set; } = EstadoUsuario.ACTIVO;

    public virtual Tienda Tienda { get; set; } = null!;
    public virtual Categoria? Categoria { get; set; }
    public virtual ICollection<DetalleVenta> DetalleVentas { get; set; } = new List<DetalleVenta>();
    public virtual ICollection<DetalleCompra> DetalleCompras { get; set; } = new List<DetalleCompra>();
}
