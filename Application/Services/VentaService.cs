using AutoMapper;
using back_tienda.Core.DTOs;
using back_tienda.Core.Entities;
using back_tienda.Core.Enums;
using back_tienda.Core.Interfaces;

namespace back_tienda.Application.Services;

public class VentaService : IVentaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public VentaService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<VentaDto>> GetAllAsync()
    {
        var ventas = await _unitOfWork.Ventas.GetAllAsync();
        var ventasConDetalles = new List<VentaDto>();
        
        foreach (var venta in ventas)
        {
            var ventaCompleta = await _unitOfWork.Ventas.GetWithDetallesAsync(venta.IdVenta);
            if (ventaCompleta != null)
            {
                ventasConDetalles.Add(_mapper.Map<VentaDto>(ventaCompleta));
            }
        }
        
        return ventasConDetalles;
    }

    public async Task<VentaDto?> GetByIdAsync(Guid id)
    {
        var venta = await _unitOfWork.Ventas.GetWithDetallesAsync(id);
        return venta != null ? _mapper.Map<VentaDto>(venta) : null;
    }

    public async Task<IEnumerable<VentaDto>> GetByTiendaAsync(Guid idTienda)
    {
        var ventas = await _unitOfWork.Ventas.GetByTiendaAsync(idTienda);
        var ventasConDetalles = new List<VentaDto>();

        foreach (var venta in ventas)
        {
            var ventaCompleta = await _unitOfWork.Ventas.GetWithDetallesAsync(venta.IdVenta);
            if (ventaCompleta != null)
            {
                ventasConDetalles.Add(_mapper.Map<VentaDto>(ventaCompleta));
            }
        }

        return ventasConDetalles;
    }

    public async Task<VentaDto> CrearAsync(Guid idUsuario, CrearVentaDto dto)
    {
        // Verificar que la tienda existe
        var tienda = await _unitOfWork.Tiendas.GetByIdAsync(dto.IdTienda);
        if (tienda == null)
        {
            throw new KeyNotFoundException("Tienda no encontrada");
        }

        // Iniciar transacción
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Agrupar detalles por producto para evitar duplicados y sumar cantidades
            var detallesAgrupados = dto.Detalles
                .GroupBy(d => d.IdProducto)
                .Select(g => new
                {
                    IdProducto = g.Key,
                    CantidadTotal = g.Sum(d => d.Cantidad),
                    Detalles = g.ToList()
                })
                .ToList();

            // Validar stock disponible para todos los productos
            foreach (var detalleAgrupado in detallesAgrupados)
            {
                var producto = await _unitOfWork.Productos.GetByIdAsync(detalleAgrupado.IdProducto);

                if (producto == null)
                {
                    throw new KeyNotFoundException($"Producto {detalleAgrupado.IdProducto} no encontrado");
                }

                if (producto.IdTienda != dto.IdTienda)
                {
                    throw new InvalidOperationException($"El producto {producto.NombreProducto} no pertenece a esta tienda");
                }

                if (producto.StockActual < detalleAgrupado.CantidadTotal)
                {
                    throw new InvalidOperationException(
                        $"Stock insuficiente para {producto.NombreProducto}. " +
                        $"Disponible: {producto.StockActual}, Solicitado: {detalleAgrupado.CantidadTotal}");
                }
            }

            // Crear la venta (cabecera de factura)
            var venta = new Venta
            {
                IdTienda = dto.IdTienda,
                IdUsuario = idUsuario,
                MetodoPago = dto.MetodoPago,
                Observaciones = dto.Observaciones,
                FechaVenta = DateTime.UtcNow,
                Estado = EstadoDocumento.ACTIVO
            };

            decimal totalVenta = 0;

            // Agregar detalles de venta (productos de la factura) - usar detalles agrupados para evitar duplicados
            foreach (var detalleAgrupado in detallesAgrupados)
            {
                var producto = await _unitOfWork.Productos.GetByIdAsync(detalleAgrupado.IdProducto);

                // Usar el precio actual del producto (no el que viene del DTO)
                var precioUnitario = producto!.PrecioVenta;

                // Calcular subtotal para la cantidad total
                var subtotal = detalleAgrupado.CantidadTotal * precioUnitario;

                var detalle = new DetalleVenta
                {
                    IdProducto = detalleAgrupado.IdProducto,
                    Cantidad = detalleAgrupado.CantidadTotal,
                    PrecioUnitario = precioUnitario,
                    Subtotal = subtotal
                };

                venta.DetalleVentas.Add(detalle);
                totalVenta += subtotal;

                // Actualizar stock del producto (solo una vez por producto)
                producto.StockActual -= detalleAgrupado.CantidadTotal;
                producto.FechaUltimaModificacion = DateTime.UtcNow;
                await _unitOfWork.Productos.UpdateAsync(producto);
            }

            // Asignar total calculado
            venta.TotalVenta = totalVenta;

            // Guardar venta
            await _unitOfWork.Ventas.AddAsync(venta);
            await _unitOfWork.SaveChangesAsync();

            // Confirmar transacción
            await _unitOfWork.CommitTransactionAsync();

            // Obtener venta completa con relaciones
            var ventaCompleta = await _unitOfWork.Ventas.GetWithDetallesAsync(venta.IdVenta);
            return _mapper.Map<VentaDto>(ventaCompleta!);
        }
        catch
        {
            // Revertir transacción en caso de error
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<bool> CancelarAsync(Guid id, Guid idUsuario, string motivo)
    {
        var venta = await _unitOfWork.Ventas.GetWithDetallesAsync(id);

        if (venta == null)
        {
            throw new KeyNotFoundException("Venta no encontrada");
        }

        if (venta.Estado == EstadoDocumento.CANCELADO)
        {
            throw new InvalidOperationException("La venta ya está cancelada");
        }

        // Iniciar transacción
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Revertir stock de todos los productos
            foreach (var detalle in venta.DetalleVentas)
            {
                var producto = await _unitOfWork.Productos.GetByIdAsync(detalle.IdProducto);
                if (producto != null)
                {
                    producto.StockActual += detalle.Cantidad;
                    producto.FechaUltimaModificacion = DateTime.UtcNow;
                    await _unitOfWork.Productos.UpdateAsync(producto);
                }
            }

            // Marcar venta como cancelada
            venta.Estado = EstadoDocumento.CANCELADO;
            venta.Observaciones = $"{venta.Observaciones}\n[CANCELADA] {motivo} - Por: {idUsuario} - Fecha: {DateTime.UtcNow:dd/MM/yyyy HH:mm}";

            await _unitOfWork.Ventas.UpdateAsync(venta);
            await _unitOfWork.SaveChangesAsync();

            // Confirmar transacción
            await _unitOfWork.CommitTransactionAsync();

            return true;
        }
        catch
        {
            // Revertir transacción en caso de error
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<VentaDto> ActualizarAsync(Guid id, Guid idUsuario, ActualizarVentaDto dto)
    {
        var venta = await _unitOfWork.Ventas.GetWithDetallesAsync(id);

        if (venta == null)
        {
            throw new KeyNotFoundException("Venta no encontrada");
        }

        if (venta.Estado == EstadoDocumento.CANCELADO)
        {
            throw new InvalidOperationException("No se puede actualizar una venta cancelada");
        }

        // Actualizar campos permitidos
        if (!string.IsNullOrEmpty(dto.MetodoPago))
        {
            venta.MetodoPago = dto.MetodoPago;
        }

        if (dto.Observaciones != null)
        {
            venta.Observaciones = dto.Observaciones;
        }

        // Marcar como editada si no estaba editada
        if (venta.Estado == EstadoDocumento.ACTIVO)
        {
            venta.Estado = EstadoDocumento.EDITADO;
        }

        await _unitOfWork.Ventas.UpdateAsync(venta);
        await _unitOfWork.SaveChangesAsync();

        // Obtener venta completa con relaciones
        var ventaCompleta = await _unitOfWork.Ventas.GetWithDetallesAsync(venta.IdVenta);
        return _mapper.Map<VentaDto>(ventaCompleta!);
    }
}
