using back_tienda.Core.Enums;

namespace back_tienda.Core.DTOs;

// DTOs de Venta
public record VentaDto(
    Guid IdVenta,
    Guid IdTienda,
    string NombreTienda,
    Guid IdUsuario,
    string NombreUsuario,
    DateTime FechaVenta,
    decimal TotalVenta,
    string? MetodoPago,
    string? Observaciones,
    EstadoDocumento Estado,
    List<DetalleVentaDto> Detalles
)
{
    public Guid Id => IdVenta;
};


public record CrearVentaDto(
    Guid IdTienda,
    string? MetodoPago,
    string? Observaciones,
    List<CrearDetalleVentaDto> Detalles
);

public record DetalleVentaDto(
    Guid IdDetalleVenta,
    Guid IdProducto,
    string NombreProducto,
    int Cantidad,
    decimal PrecioUnitario,
    decimal Subtotal
);

public record CrearDetalleVentaDto(
    Guid IdProducto,
    int Cantidad,
    decimal PrecioUnitario
);

// DTOs de Compra
public record CompraDto(
    Guid IdCompra,
    Guid IdTienda,
    string NombreTienda,
    Guid? IdProveedor,
    string? NombreProveedor,
    Guid IdUsuario,
    string NombreUsuario,
    string? NumeroFactura,
    DateTime FechaCompra,
    decimal TotalCompra,
    string? Observaciones,
    EstadoDocumento Estado,
    bool PuedeEditar,
    DateTime? FechaLimiteEdicion,
    List<DetalleCompraDto> Detalles
)
{
    public Guid Id => IdCompra;
};


public record CrearCompraDto(
    Guid? IdTienda,
    Guid? IdProveedor,
    string? NumeroFactura,
    string? Observaciones,
    List<CrearDetalleCompraDto> Detalles
);

public record DetalleCompraDto(
    Guid IdDetalleCompra,
    Guid IdProducto,
    string NombreProducto,
    int Cantidad,
    decimal PrecioUnitario,
    decimal Subtotal
);

public record CrearDetalleCompraDto(
    Guid IdProducto,
    int Cantidad,
    decimal PrecioUnitario
);

// DTOs de Devolución
public record DevolucionDto(
    Guid IdDevolucion,
    Guid IdVenta,
    Guid IdProducto,
    string NombreProducto,
    Guid IdUsuario,
    string NombreUsuario,
    int Cantidad,
    string? Motivo,
    DateTime FechaDevolucion,
    decimal? MontoDevuelto
)
{
    public Guid Id => IdDevolucion;
};


public record CrearDevolucionDto(
    Guid IdVenta,
    Guid IdProducto,
    int Cantidad,
    string? Motivo,
    decimal? MontoDevuelto
);

// DTOs de Merma
public record MermaDto(
    Guid IdMerma,
    Guid IdTienda,
    Guid IdProducto,
    string NombreProducto,
    Guid IdUsuario,
    string NombreUsuario,
    int Cantidad,
    string? Motivo,
    string? Descripcion,
    DateTime FechaMerma
)
{
    public Guid Id => IdMerma;
};


public record CrearMermaDto(
    Guid IdTienda,
    Guid IdProducto,
    int Cantidad,
    string? Motivo,
    string? Descripcion
);

// DTOs de Reportes
public record ReporteVentasDto(
    DateTime FechaInicio,
    DateTime FechaFin,
    int TotalVentas,
    decimal TotalIngresos,
    List<ProductoMasVendidoDto> ProductosMasVendidos
);

public record ProductoMasVendidoDto(
    Guid IdProducto,
    string NombreProducto,
    int CantidadVendida,
    decimal TotalIngresos
);

public record DashboardDto(
    int TotalProductos,
    int ProductosStockBajo,
    decimal VentasHoy,
    decimal VentasMes,
    string? ProductoMasVendido,
    DateTime? UltimaVenta
);

public record AlertaStockDto(
    Guid IdProducto,
    string NombreProducto,
    string? CodigoProducto,
    int StockActual,
    int StockMinimo,
    DateTime FechaAlerta
);

public record ReporteDto(
    int VentasTotales,
    int ComprasTotales,
    decimal Ganancia,
    int ProductosVendidos,
    List<VentaDiaDto> VentasPorDia
);

public record VentaDiaDto(
    string Fecha,
    decimal Total
);
