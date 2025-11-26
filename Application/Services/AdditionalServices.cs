using AutoMapper;
using back_tienda.Core.DTOs;
using back_tienda.Core.Entities;
using back_tienda.Core.Interfaces;

namespace back_tienda.Application.Services;

public class ReporteService : IReporteService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ReporteService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<DashboardDto> GetDashboardAsync(Guid idTienda)
    {
        var productos = await _unitOfWork.Productos.GetByTiendaAsync(idTienda);
        var productosStockBajo = await _unitOfWork.Productos.GetProductosStockBajoAsync(idTienda);
        
        var hoy = DateTime.UtcNow.Date;
        var ventasHoy = await _unitOfWork.Ventas.GetTotalVentasDiaAsync(idTienda, hoy);
        var ventasMes = await _unitOfWork.Ventas.GetTotalVentasMesAsync(idTienda, DateTime.UtcNow.Year, DateTime.UtcNow.Month);

        var ventas = await _unitOfWork.Ventas.GetByTiendaAsync(idTienda);
        var ultimaVenta = ventas.OrderByDescending(v => v.FechaVenta).FirstOrDefault();

        // Producto más vendido del mes
        var ventasMesActual = await _unitOfWork.Ventas.GetByFechaRangoAsync(
            idTienda,
            new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1),
            DateTime.UtcNow
        );

        var productoMasVendido = ventasMesActual
            .SelectMany(v => v.DetalleVentas)
            .GroupBy(d => d.Producto.NombreProducto)
            .OrderByDescending(g => g.Sum(d => d.Cantidad))
            .FirstOrDefault()?.Key;

        return new DashboardDto(
            TotalProductos: productos.Count(),
            ProductosStockBajo: productosStockBajo.Count(),
            VentasHoy: ventasHoy,
            VentasMes: ventasMes,
            ProductoMasVendido: productoMasVendido,
            UltimaVenta: ultimaVenta?.FechaVenta
        );
    }

    public async Task<ReporteDto> GetResumenAsync(Guid idTienda)
    {
        var ventas = await _unitOfWork.Ventas.GetByTiendaAsync(idTienda);
        var compras = await _unitOfWork.Compras.GetByTiendaAsync(idTienda);
        
        var ventasList = ventas.ToList();
        var comprasList = compras.ToList();

        var ventasTotales = ventasList.Count;
        var comprasTotales = comprasList.Count;
        
        var totalVentas = ventasList.Sum(v => v.TotalVenta);
        var totalCompras = comprasList.Sum(c => c.TotalCompra);
        var ganancia = totalVentas - totalCompras;
        
        var productosVendidos = ventasList
            .SelectMany(v => v.DetalleVentas)
            .Sum(d => d.Cantidad);

        var ventasPorDia = ventasList
            .GroupBy(v => v.FechaVenta.Date)
            .Select(g => new VentaDiaDto(
                Fecha: g.Key.ToString("yyyy-MM-dd"),
                Total: g.Sum(v => v.TotalVenta)
            ))
            .OrderBy(v => v.Fecha)
            .ToList();

        return new ReporteDto(
            VentasTotales: ventasTotales,
            ComprasTotales: comprasTotales,
            Ganancia: ganancia,
            ProductosVendidos: productosVendidos,
            VentasPorDia: ventasPorDia
        );
    }

    public async Task<ReporteVentasDto> GetReporteVentasAsync(Guid idTienda, DateTime inicio, DateTime fin)
    {
        var ventas = await _unitOfWork.Ventas.GetByFechaRangoAsync(idTienda, inicio, fin);
        var ventasList = ventas.ToList();

        var totalVentas = ventasList.Count;
        var totalIngresos = ventasList.Sum(v => v.TotalVenta);

        var productosMasVendidos = ventasList
            .SelectMany(v => v.DetalleVentas)
            .GroupBy(d => new { d.IdProducto, d.Producto.NombreProducto })
            .Select(g => new ProductoMasVendidoDto(
                IdProducto: g.Key.IdProducto,
                NombreProducto: g.Key.NombreProducto,
                CantidadVendida: g.Sum(d => d.Cantidad),
                TotalIngresos: g.Sum(d => d.Subtotal)
            ))
            .OrderByDescending(p => p.CantidadVendida)
            .Take(10)
            .ToList();

        return new ReporteVentasDto(
            FechaInicio: inicio,
            FechaFin: fin,
            TotalVentas: totalVentas,
            TotalIngresos: totalIngresos,
            ProductosMasVendidos: productosMasVendidos
        );
    }

    public async Task<IEnumerable<AlertaStockDto>> GetAlertasStockAsync(Guid idTienda)
    {
        var productosStockBajo = await _unitOfWork.Productos.GetProductosStockBajoAsync(idTienda);

        return productosStockBajo.Select(p => new AlertaStockDto(
            IdProducto: p.IdProducto,
            NombreProducto: p.NombreProducto,
            CodigoProducto: p.CodigoProducto,
            StockActual: p.StockActual,
            StockMinimo: p.StockMinimo,
            FechaAlerta: DateTime.UtcNow
        ));
    }
}

public class DevolucionService : IDevolucionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public DevolucionService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<DevolucionDto> CrearAsync(Guid idUsuario, CrearDevolucionDto dto)
    {
        var venta = await _unitOfWork.Ventas.GetWithDetallesAsync(dto.IdVenta);
        if (venta == null)
        {
            throw new KeyNotFoundException("Venta no encontrada");
        }

        // Verificar que el producto existe en la venta
        var detalleVenta = venta.DetalleVentas.FirstOrDefault(d => d.IdProducto == dto.IdProducto);
        if (detalleVenta == null)
        {
            throw new InvalidOperationException("El producto no existe en esta venta");
        }

        // Verificar cantidad ya devuelta
        var cantidadYaDevuelta = await _unitOfWork.Devoluciones.GetCantidadDevueltaAsync(dto.IdVenta, dto.IdProducto);
        var cantidadDisponible = detalleVenta.Cantidad - cantidadYaDevuelta;

        if (dto.Cantidad > cantidadDisponible)
        {
            throw new InvalidOperationException(
                $"No se puede devolver más de lo vendido. Disponible para devolución: {cantidadDisponible}");
        }

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var devolucion = new Devolucion
            {
                IdVenta = dto.IdVenta,
                IdProducto = dto.IdProducto,
                IdUsuario = idUsuario,
                Cantidad = dto.Cantidad,
                Motivo = dto.Motivo,
                MontoDevuelto = dto.MontoDevuelto,
                FechaDevolucion = DateTime.UtcNow
            };

            // Actualizar stock
            var producto = await _unitOfWork.Productos.GetByIdAsync(dto.IdProducto);
            if (producto != null)
            {
                producto.StockActual += dto.Cantidad;
                await _unitOfWork.Productos.UpdateAsync(producto);
            }

            await _unitOfWork.Devoluciones.AddAsync(devolucion);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return _mapper.Map<DevolucionDto>(devolucion);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<IEnumerable<DevolucionDto>> GetByVentaAsync(Guid idVenta)
    {
        var devoluciones = await _unitOfWork.Devoluciones.GetByVentaAsync(idVenta);
        return _mapper.Map<IEnumerable<DevolucionDto>>(devoluciones);
    }
}

public class MermaService : IMermaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public MermaService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<MermaDto> CrearAsync(Guid idUsuario, CrearMermaDto dto)
    {
        var producto = await _unitOfWork.Productos.GetByIdAsync(dto.IdProducto);
        if (producto == null)
        {
            throw new KeyNotFoundException("Producto no encontrado");
        }

        if (producto.IdTienda != dto.IdTienda)
        {
            throw new InvalidOperationException("El producto no pertenece a esta tienda");
        }

        if (producto.StockActual < dto.Cantidad)
        {
            throw new InvalidOperationException(
                $"Stock insuficiente. Disponible: {producto.StockActual}, Solicitado: {dto.Cantidad}");
        }

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var merma = new Merma
            {
                IdTienda = dto.IdTienda,
                IdProducto = dto.IdProducto,
                IdUsuario = idUsuario,
                Cantidad = dto.Cantidad,
                Motivo = dto.Motivo,
                Descripcion = dto.Descripcion,
                FechaMerma = DateTime.UtcNow
            };

            // Actualizar stock
            producto.StockActual -= dto.Cantidad;
            await _unitOfWork.Productos.UpdateAsync(producto);

            await _unitOfWork.Mermas.AddAsync(merma);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return _mapper.Map<MermaDto>(merma);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<IEnumerable<MermaDto>> GetByTiendaAsync(Guid idTienda)
    {
        var mermas = await _unitOfWork.Mermas.GetByTiendaAsync(idTienda);
        return _mapper.Map<IEnumerable<MermaDto>>(mermas);
    }
}

public class UsuarioService : IUsuarioService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UsuarioService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<UsuarioDto?> GetByIdAsync(Guid id)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id);
        return usuario != null ? _mapper.Map<UsuarioDto>(usuario) : null;
    }

    public async Task<IEnumerable<UsuarioDto>> GetAllAsync()
    {
        var usuarios = await _unitOfWork.Usuarios.GetAllAsync();
        return _mapper.Map<IEnumerable<UsuarioDto>>(usuarios);
    }

    public async Task<UsuarioDto> ActualizarAsync(Guid id, ActualizarUsuarioDto dto)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id);
        if (usuario == null)
        {
            throw new KeyNotFoundException("Usuario no encontrado");
        }

        if (!string.IsNullOrEmpty(dto.NombreCompleto))
            usuario.NombreCompleto = dto.NombreCompleto;

        if (!string.IsNullOrEmpty(dto.Correo))
        {
            if (await _unitOfWork.Usuarios.ExisteCorreoAsync(dto.Correo) && usuario.Correo != dto.Correo)
            {
                throw new InvalidOperationException("El correo ya está en uso");
            }
            usuario.Correo = dto.Correo;
        }

        if (dto.Estado.HasValue)
        {
            usuario.Estado = dto.Estado.Value;
        }

        if (dto.Rol.HasValue)
        {
            usuario.Rol = dto.Rol.Value;
        }

        usuario.FechaUltimaModificacion = DateTime.UtcNow;

        await _unitOfWork.Usuarios.UpdateAsync(usuario);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<UsuarioDto>(usuario);
    }

    public async Task<bool> EliminarAsync(Guid id)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id);
        if (usuario == null) return false;

        usuario.Estado = Core.Enums.EstadoUsuario.INACTIVO;
        await _unitOfWork.Usuarios.UpdateAsync(usuario);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
