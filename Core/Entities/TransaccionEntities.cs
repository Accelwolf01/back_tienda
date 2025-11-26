using back_tienda.Core.Enums;

namespace back_tienda.Core.Entities;

public class Venta
{
    public Guid IdVenta { get; set; }
    public Guid IdTienda { get; set; }
    public Guid IdUsuario { get; set; }
    public DateTime FechaVenta { get; set; } = DateTime.UtcNow;
    public decimal TotalVenta { get; set; } = 0;
    public string? MetodoPago { get; set; }
    public string? Observaciones { get; set; }
    public EstadoDocumento Estado { get; set; } = EstadoDocumento.ACTIVO;

    public virtual Tienda Tienda { get; set; } = null!;
    public virtual Usuario Usuario { get; set; } = null!;
    public virtual ICollection<DetalleVenta> DetalleVentas { get; set; } = new List<DetalleVenta>();
}

public class DetalleVenta
{
    public Guid IdDetalleVenta { get; set; }
    public Guid IdVenta { get; set; }
    public Guid IdProducto { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }

    public virtual Venta Venta { get; set; } = null!;
    public virtual Producto Producto { get; set; } = null!;
}

public class Compra
{
    public Guid IdCompra { get; set; }
    public Guid IdTienda { get; set; }
    public Guid? IdProveedor { get; set; }
    public Guid IdUsuario { get; set; }
    public string? NumeroFactura { get; set; }
    public DateTime FechaCompra { get; set; } = DateTime.UtcNow;
    public decimal TotalCompra { get; set; } = 0;
    public string? Observaciones { get; set; }
    public EstadoDocumento Estado { get; set; } = EstadoDocumento.ACTIVO;
    public bool PuedeEditar { get; set; } = true;
    public DateTime? FechaLimiteEdicion { get; set; }

    public virtual Tienda Tienda { get; set; } = null!;
    public virtual Proveedor? Proveedor { get; set; }
    public virtual Usuario Usuario { get; set; } = null!;
    public virtual ICollection<DetalleCompra> DetalleCompras { get; set; } = new List<DetalleCompra>();
}

public class DetalleCompra
{
    public Guid IdDetalleCompra { get; set; }
    public Guid IdCompra { get; set; }
    public Guid IdProducto { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }

    public virtual Compra Compra { get; set; } = null!;
    public virtual Producto Producto { get; set; } = null!;
}

public class Devolucion
{
    public Guid IdDevolucion { get; set; }
    public Guid IdVenta { get; set; }
    public Guid IdProducto { get; set; }
    public Guid IdUsuario { get; set; }
    public int Cantidad { get; set; }
    public string? Motivo { get; set; }
    public DateTime FechaDevolucion { get; set; } = DateTime.UtcNow;
    public decimal? MontoDevuelto { get; set; }

    public virtual Venta Venta { get; set; } = null!;
    public virtual Producto Producto { get; set; } = null!;
    public virtual Usuario Usuario { get; set; } = null!;
}

public class Merma
{
    public Guid IdMerma { get; set; }
    public Guid IdTienda { get; set; }
    public Guid IdProducto { get; set; }
    public Guid IdUsuario { get; set; }
    public int Cantidad { get; set; }
    public string? Motivo { get; set; }
    public string? Descripcion { get; set; }
    public DateTime FechaMerma { get; set; } = DateTime.UtcNow;

    public virtual Tienda Tienda { get; set; } = null!;
    public virtual Producto Producto { get; set; } = null!;
    public virtual Usuario Usuario { get; set; } = null!;
}
