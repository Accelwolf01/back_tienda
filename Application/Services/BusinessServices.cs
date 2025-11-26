using AutoMapper;
using back_tienda.Core.DTOs;
using back_tienda.Core.Entities;
using back_tienda.Core.Enums;
using back_tienda.Core.Interfaces;

namespace back_tienda.Application.Services;

public class CompraService : ICompraService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CompraService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CompraDto>> GetAllAsync()
    {
        var compras = await _unitOfWork.Compras.GetAllAsync();
        var comprasConDetalles = new List<CompraDto>();
        
        foreach (var compra in compras)
        {
            var compraCompleta = await _unitOfWork.Compras.GetWithDetallesAsync(compra.IdCompra);
            if (compraCompleta != null)
            {
                comprasConDetalles.Add(_mapper.Map<CompraDto>(compraCompleta));
            }
        }
        
        return comprasConDetalles;
    }

    public async Task<CompraDto?> GetByIdAsync(Guid id)
    {
        var compra = await _unitOfWork.Compras.GetWithDetallesAsync(id);
        return compra != null ? _mapper.Map<CompraDto>(compra) : null;
    }

    public async Task<IEnumerable<CompraDto>> GetByTiendaAsync(Guid idTienda)
    {
        var compras = await _unitOfWork.Compras.GetByTiendaAsync(idTienda);
        var comprasConDetalles = new List<CompraDto>();

        foreach (var compra in compras)
        {
            var compraCompleta = await _unitOfWork.Compras.GetWithDetallesAsync(compra.IdCompra);
            if (compraCompleta != null)
            {
                comprasConDetalles.Add(_mapper.Map<CompraDto>(compraCompleta));
            }
        }

        return comprasConDetalles;
    }

    public async Task<CompraDto> CrearAsync(Guid idUsuario, CrearCompraDto dto)
    {
        if (!dto.IdTienda.HasValue)
        {
            throw new ArgumentException("IdTienda es requerido");
        }

        var idTienda = dto.IdTienda.Value;
        var tienda = await _unitOfWork.Tiendas.GetByIdAsync(idTienda);
        if (tienda == null)
        {
            throw new KeyNotFoundException("Tienda no encontrada");
        }

        if (dto.IdProveedor.HasValue)
        {
            var proveedor = await _unitOfWork.Proveedores.GetByIdAsync(dto.IdProveedor.Value);
            if (proveedor == null || proveedor.IdTienda != dto.IdTienda)
            {
                throw new InvalidOperationException("Proveedor no válido para esta tienda");
            }
        }

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Validar productos
            foreach (var detalleDto in dto.Detalles)
            {
                var producto = await _unitOfWork.Productos.GetByIdAsync(detalleDto.IdProducto);
                if (producto == null || producto.IdTienda != dto.IdTienda)
                {
                    throw new InvalidOperationException($"Producto {detalleDto.IdProducto} no válido para esta tienda");
                }
            }

            // Crear compra
            var compra = new Compra
            {
                IdTienda = idTienda,
                IdProveedor = dto.IdProveedor,
                IdUsuario = idUsuario,
                NumeroFactura = dto.NumeroFactura,
                Observaciones = dto.Observaciones,
                FechaCompra = DateTime.UtcNow,
                Estado = EstadoDocumento.ACTIVO,
                PuedeEditar = true,
                FechaLimiteEdicion = DateTime.UtcNow.AddHours(24)
            };

            decimal totalCompra = 0;

            // Agregar detalles y actualizar stock
            foreach (var detalleDto in dto.Detalles)
            {
                var producto = await _unitOfWork.Productos.GetByIdAsync(detalleDto.IdProducto);
                var subtotal = detalleDto.Cantidad * detalleDto.PrecioUnitario;

                var detalle = new DetalleCompra
                {
                    IdProducto = detalleDto.IdProducto,
                    Cantidad = detalleDto.Cantidad,
                    PrecioUnitario = detalleDto.PrecioUnitario,
                    Subtotal = subtotal
                };

                compra.DetalleCompras.Add(detalle);
                totalCompra += subtotal;

                // Actualizar stock y precio de compra
                producto!.StockActual += detalleDto.Cantidad;
                producto.PrecioCompra = detalleDto.PrecioUnitario;
                producto.FechaUltimaModificacion = DateTime.UtcNow;
                await _unitOfWork.Productos.UpdateAsync(producto);
            }

            compra.TotalCompra = totalCompra;

            await _unitOfWork.Compras.AddAsync(compra);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            var compraCompleta = await _unitOfWork.Compras.GetWithDetallesAsync(compra.IdCompra);
            return _mapper.Map<CompraDto>(compraCompleta!);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<CompraDto> ActualizarAsync(Guid id, CrearCompraDto dto)
    {
        var compra = await _unitOfWork.Compras.GetWithDetallesAsync(id);
        
        if (compra == null)
        {
            throw new KeyNotFoundException("Compra no encontrada");
        }

        // Verificar si puede editar (24 horas)
        if (!await _unitOfWork.Compras.PuedeEditarAsync(id))
        {
            throw new InvalidOperationException("La compra ya no puede ser editada (límite de 24 horas excedido)");
        }

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Revertir stock de la compra original
            foreach (var detalle in compra.DetalleCompras)
            {
                var producto = await _unitOfWork.Productos.GetByIdAsync(detalle.IdProducto);
                if (producto != null)
                {
                    producto.StockActual -= detalle.Cantidad;
                    await _unitOfWork.Productos.UpdateAsync(producto);
                }
            }

            // Limpiar detalles antiguos
            compra.DetalleCompras.Clear();

            // Agregar nuevos detalles
            decimal totalCompra = 0;
            foreach (var detalleDto in dto.Detalles)
            {
                var producto = await _unitOfWork.Productos.GetByIdAsync(detalleDto.IdProducto);
                var subtotal = detalleDto.Cantidad * detalleDto.PrecioUnitario;

                var detalle = new DetalleCompra
                {
                    IdCompra = id,
                    IdProducto = detalleDto.IdProducto,
                    Cantidad = detalleDto.Cantidad,
                    PrecioUnitario = detalleDto.PrecioUnitario,
                    Subtotal = subtotal
                };

                compra.DetalleCompras.Add(detalle);
                totalCompra += subtotal;

                // Actualizar stock
                producto!.StockActual += detalleDto.Cantidad;
                producto.PrecioCompra = detalleDto.PrecioUnitario;
                await _unitOfWork.Productos.UpdateAsync(producto);
            }

            compra.TotalCompra = totalCompra;
            compra.Estado = EstadoDocumento.EDITADO;
            compra.Observaciones = dto.Observaciones;

            await _unitOfWork.Compras.UpdateAsync(compra);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            var compraActualizada = await _unitOfWork.Compras.GetWithDetallesAsync(id);
            return _mapper.Map<CompraDto>(compraActualizada!);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}

public class TiendaService : ITiendaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TiendaService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<TiendaDto?> GetByIdAsync(Guid id)
    {
        var tienda = await _unitOfWork.Tiendas.GetByIdAsync(id);
        return tienda != null ? _mapper.Map<TiendaDto>(tienda) : null;
    }

    public async Task<IEnumerable<TiendaDto>> GetAllAsync()
    {
        var tiendas = await _unitOfWork.Tiendas.GetAllAsync();
        return _mapper.Map<IEnumerable<TiendaDto>>(tiendas);
    }

    public async Task<IEnumerable<TiendaDto>> GetByDueñoAsync(Guid idDueño)
    {
        var tiendas = await _unitOfWork.Tiendas.GetByDueñoAsync(idDueño);
        return _mapper.Map<IEnumerable<TiendaDto>>(tiendas);
    }

    public async Task<TiendaDto> CrearAsync(CrearTiendaDto dto)
    {
        var dueño = await _unitOfWork.Usuarios.GetByIdAsync(dto.IdDueño);
        if (dueño == null)
        {
            throw new KeyNotFoundException("Usuario dueño no encontrado");
        }

        if (dueño.Rol != Core.Enums.TipoRol.DUEÑO_TIENDA && dueño.Rol != Core.Enums.TipoRol.ADMIN_SISTEMA)
        {
            throw new InvalidOperationException("El usuario debe tener rol DUEÑO_TIENDA o ADMIN_SISTEMA");
        }

        var tienda = _mapper.Map<Tienda>(dto);
        tienda.FechaCreacion = DateTime.UtcNow;

        await _unitOfWork.Tiendas.AddAsync(tienda);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<TiendaDto>(tienda);
    }

    public async Task<TiendaDto> ActualizarAsync(Guid id, ActualizarTiendaDto dto)
    {
        var tienda = await _unitOfWork.Tiendas.GetByIdAsync(id);
        if (tienda == null)
        {
            throw new KeyNotFoundException("Tienda no encontrada");
        }

        if (!string.IsNullOrEmpty(dto.NombreTienda))
            tienda.NombreTienda = dto.NombreTienda;
        
        if (dto.Direccion != null)
            tienda.Direccion = dto.Direccion;
        
        if (dto.Telefono != null)
            tienda.Telefono = dto.Telefono;
        
        if (dto.CorreoTienda != null)
            tienda.CorreoTienda = dto.CorreoTienda;
        
        if (dto.Nit != null)
            tienda.Nit = dto.Nit;

        if (dto.Estado.HasValue)
            tienda.Estado = dto.Estado.Value;

        tienda.FechaUltimaModificacion = DateTime.UtcNow;

        await _unitOfWork.Tiendas.UpdateAsync(tienda);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<TiendaDto>(tienda);
    }

    public async Task<bool> EliminarAsync(Guid id)
    {
        var tienda = await _unitOfWork.Tiendas.GetByIdAsync(id);
        if (tienda == null)
        {
            return false;
        }

        tienda.Estado = Core.Enums.EstadoUsuario.INACTIVO;
        await _unitOfWork.Tiendas.UpdateAsync(tienda);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}

public class CategoriaService : ICategoriaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CategoriaService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CategoriaDto>> GetByTiendaAsync(Guid idTienda)
    {
        var categorias = await _unitOfWork.Categorias.GetByTiendaAsync(idTienda);
        return _mapper.Map<IEnumerable<CategoriaDto>>(categorias);
    }

    public async Task<CategoriaDto> CrearAsync(CrearCategoriaDto dto)
    {
        if (await _unitOfWork.Categorias.ExisteNombreEnTiendaAsync(dto.IdTienda, dto.NombreCategoria))
        {
            throw new InvalidOperationException("Ya existe una categoría con ese nombre en esta tienda");
        }

        var categoria = _mapper.Map<Categoria>(dto);
        categoria.FechaCreacion = DateTime.UtcNow;

        await _unitOfWork.Categorias.AddAsync(categoria);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CategoriaDto>(categoria);
    }

    public async Task<bool> EliminarAsync(Guid id)
    {
        await _unitOfWork.Categorias.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}

public class ProveedorService : IProveedorService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProveedorService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProveedorDto>> GetByTiendaAsync(Guid idTienda)
    {
        var proveedores = await _unitOfWork.Proveedores.GetByTiendaAsync(idTienda);
        return _mapper.Map<IEnumerable<ProveedorDto>>(proveedores);
    }

    public async Task<ProveedorDto> CrearAsync(CrearProveedorDto dto)
    {
        if (await _unitOfWork.Proveedores.ExisteNombreEnTiendaAsync(dto.IdTienda, dto.NombreProveedor))
        {
            throw new InvalidOperationException("Ya existe un proveedor con ese nombre en esta tienda");
        }

        var proveedor = _mapper.Map<Proveedor>(dto);
        proveedor.FechaCreacion = DateTime.UtcNow;

        await _unitOfWork.Proveedores.AddAsync(proveedor);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ProveedorDto>(proveedor);
    }

    public async Task<bool> EliminarAsync(Guid id)
    {
        var proveedor = await _unitOfWork.Proveedores.GetByIdAsync(id);
        if (proveedor == null) return false;

        proveedor.Estado = Core.Enums.EstadoUsuario.INACTIVO;
        await _unitOfWork.Proveedores.UpdateAsync(proveedor);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
