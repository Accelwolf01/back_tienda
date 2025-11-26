using AutoMapper;
using back_tienda.Core.DTOs;
using back_tienda.Core.Entities;
using back_tienda.Core.Interfaces;

namespace back_tienda.Application.Services;

public class ProductoService : IProductoService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProductoService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProductoDto>> GetAllAsync()
    {
        var productos = await _unitOfWork.Productos.GetAllAsync();
        return _mapper.Map<IEnumerable<ProductoDto>>(productos);
    }

    public async Task<ProductoDto?> GetByIdAsync(Guid id)
    {
        var producto = await _unitOfWork.Productos.GetByIdAsync(id);
        return producto != null ? _mapper.Map<ProductoDto>(producto) : null;
    }

    public async Task<IEnumerable<ProductoDto>> GetByTiendaAsync(Guid idTienda)
    {
        var productos = await _unitOfWork.Productos.GetByTiendaAsync(idTienda);
        return _mapper.Map<IEnumerable<ProductoDto>>(productos);
    }

    public async Task<IEnumerable<ProductoDto>> BuscarAsync(Guid idTienda, string termino)
    {
        var productos = await _unitOfWork.Productos.BuscarProductosAsync(idTienda, termino);
        return _mapper.Map<IEnumerable<ProductoDto>>(productos);
    }

    public async Task<IEnumerable<ProductoDto>> GetStockBajoAsync(Guid idTienda)
    {
        var productos = await _unitOfWork.Productos.GetProductosStockBajoAsync(idTienda);
        return _mapper.Map<IEnumerable<ProductoDto>>(productos);
    }

    public async Task<ProductoDto> CrearAsync(CrearProductoDto dto)
    {
        if (!dto.IdTienda.HasValue)
        {
            throw new InvalidOperationException("La tienda es requerida");
        }

        var tienda = await _unitOfWork.Tiendas.GetByIdAsync(dto.IdTienda.Value);
        if (tienda == null)
        {
            throw new KeyNotFoundException("Tienda no encontrada");
        }

        var codigoProducto = dto.CodigoProducto;
        if (string.IsNullOrEmpty(codigoProducto))
        {
            codigoProducto = $"PROD-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
        }
        else
        {
            var productoExistente = await _unitOfWork.Productos.GetByCodigoAsync(dto.IdTienda.Value, codigoProducto);
            if (productoExistente != null)
            {
                throw new InvalidOperationException("Ya existe un producto con ese código en esta tienda");
            }
        }

        if (dto.IdCategoria.HasValue)
        {
            var categoria = await _unitOfWork.Categorias.GetByIdAsync(dto.IdCategoria.Value);
            if (categoria == null || categoria.IdTienda != dto.IdTienda.Value)
            {
                throw new InvalidOperationException("Categoría no válida para esta tienda");
            }
        }

        var producto = _mapper.Map<Producto>(dto);
        producto.CodigoProducto = codigoProducto;
        producto.FechaCreacion = DateTime.UtcNow;
        producto.FechaUltimaModificacion = DateTime.UtcNow;

        await _unitOfWork.Productos.AddAsync(producto);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ProductoDto>(producto);
    }

    public async Task<ProductoDto> ActualizarAsync(Guid id, ActualizarProductoDto dto)
    {
        var producto = await _unitOfWork.Productos.GetByIdAsync(id);
        if (producto == null)
        {
            throw new KeyNotFoundException("Producto no encontrado");
        }

        // Verificar categoría si se proporciona
        if (dto.IdCategoria.HasValue)
        {
            var categoria = await _unitOfWork.Categorias.GetByIdAsync(dto.IdCategoria.Value);
            if (categoria == null || categoria.IdTienda != producto.IdTienda)
            {
                throw new InvalidOperationException("Categoría no válida para esta tienda");
            }
        }

        // Actualizar solo los campos proporcionados
        if (!string.IsNullOrEmpty(dto.NombreProducto))
            producto.NombreProducto = dto.NombreProducto;
        
        if (dto.Descripcion != null)
            producto.Descripcion = dto.Descripcion;
        
        if (dto.PrecioVenta.HasValue)
            producto.PrecioVenta = dto.PrecioVenta.Value;
        
        if (dto.PrecioCompra.HasValue)
            producto.PrecioCompra = dto.PrecioCompra;
        
        if (dto.StockMinimo.HasValue)
            producto.StockMinimo = dto.StockMinimo.Value;
        
        if (dto.IdCategoria.HasValue)
            producto.IdCategoria = dto.IdCategoria;

        producto.FechaUltimaModificacion = DateTime.UtcNow;

        await _unitOfWork.Productos.UpdateAsync(producto);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ProductoDto>(producto);
    }

    public async Task<bool> EliminarAsync(Guid id)
    {
        var producto = await _unitOfWork.Productos.GetByIdAsync(id);
        if (producto == null)
        {
            return false;
        }

        // Soft delete - cambiar estado en lugar de eliminar
        producto.Estado = Core.Enums.EstadoUsuario.INACTIVO;
        producto.FechaUltimaModificacion = DateTime.UtcNow;

        await _unitOfWork.Productos.UpdateAsync(producto);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
