using AutoMapper;
using back_tienda.Core.Entities;
using back_tienda.Core.DTOs;

namespace back_tienda.Application.Mappings;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // Usuario mappings
        CreateMap<Usuario, UsuarioDto>()
            .ConstructUsing(src => new UsuarioDto(
                src.IdUsuario,
                src.NombreCompleto,
                src.UsuarioNombre,
                src.Correo,
                src.Rol,
                src.Estado
            ));
        CreateMap<RegistroUsuarioDto, Usuario>()
            .ForMember(dest => dest.UsuarioNombre, opt => opt.MapFrom(src => src.Usuario));

        // Tienda mappings
        CreateMap<Tienda, TiendaDto>()
            .ConstructUsing(src => new TiendaDto(
                src.IdTienda,
                src.NombreTienda,
                src.Direccion,
                src.Telefono,
                src.CorreoTienda,
                src.Nit,
                src.IdDueño,
                src.Dueño != null ? src.Dueño.NombreCompleto : "Sin asignar",
                src.Estado
            ));
        CreateMap<CrearTiendaDto, Tienda>();
        CreateMap<ActualizarTiendaDto, Tienda>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Producto mappings
        CreateMap<Producto, ProductoDto>()
            .ConstructUsing(src => new ProductoDto(
                src.IdProducto,
                src.IdTienda,
                src.IdCategoria,
                src.CodigoProducto,
                src.NombreProducto,
                src.Descripcion,
                src.PrecioVenta,
                src.PrecioCompra,
                src.StockActual,
                src.StockMinimo,
                src.UnidadMedida,
                src.Categoria != null ? src.Categoria.NombreCategoria : null,
                src.Estado
            ));
        CreateMap<CrearProductoDto, Producto>();
        CreateMap<ActualizarProductoDto, Producto>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Categoria mappings
        CreateMap<Categoria, CategoriaDto>()
            .ConstructUsing(src => new CategoriaDto(
                src.IdCategoria,
                src.IdTienda,
                src.NombreCategoria,
                src.Descripcion
            ));
        CreateMap<CrearCategoriaDto, Categoria>();

        // Proveedor mappings
        CreateMap<Proveedor, ProveedorDto>()
            .ConstructUsing(src => new ProveedorDto(
                src.IdProveedor,
                src.IdTienda,
                src.NombreProveedor,
                src.Nit,
                src.Telefono,
                src.Correo,
                src.Direccion,
                src.Estado
            ));
        CreateMap<CrearProveedorDto, Proveedor>();

        // Venta mappings
        CreateMap<Venta, VentaDto>()
            .ConstructUsing(src => new VentaDto(
                src.IdVenta,
                src.IdTienda,
                src.Tienda.NombreTienda,
                src.IdUsuario,
                src.Usuario.NombreCompleto,
                src.FechaVenta,
                src.TotalVenta,
                src.MetodoPago,
                src.Observaciones,
                src.Estado,
                src.DetalleVentas.Select(d => new DetalleVentaDto(
                    d.IdDetalleVenta,
                    d.IdProducto,
                    d.Producto.NombreProducto,
                    d.Cantidad,
                    d.PrecioUnitario,
                    d.Subtotal
                )).ToList()
            ));
        CreateMap<CrearVentaDto, Venta>();

        CreateMap<DetalleVenta, DetalleVentaDto>()
            .ConstructUsing(src => new DetalleVentaDto(
                src.IdDetalleVenta,
                src.IdProducto,
                src.Producto.NombreProducto,
                src.Cantidad,
                src.PrecioUnitario,
                src.Subtotal
            ));
        CreateMap<CrearDetalleVentaDto, DetalleVenta>()
            .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.Cantidad * src.PrecioUnitario));

        // Compra mappings
        CreateMap<Compra, CompraDto>()
            .ConstructUsing(src => new CompraDto(
                src.IdCompra,
                src.IdTienda,
                src.Tienda.NombreTienda,
                src.IdProveedor,
                src.Proveedor != null ? src.Proveedor.NombreProveedor : null,
                src.IdUsuario,
                src.Usuario.NombreCompleto,
                src.NumeroFactura,
                src.FechaCompra,
                src.TotalCompra,
                src.Observaciones,
                src.Estado,
                src.PuedeEditar,
                src.FechaLimiteEdicion,
                src.DetalleCompras.Select(d => new DetalleCompraDto(
                    d.IdDetalleCompra,
                    d.IdProducto,
                    d.Producto.NombreProducto,
                    d.Cantidad,
                    d.PrecioUnitario,
                    d.Subtotal
                )).ToList()
            ));
        CreateMap<CrearCompraDto, Compra>();

        CreateMap<DetalleCompra, DetalleCompraDto>()
            .ConstructUsing(src => new DetalleCompraDto(
                src.IdDetalleCompra,
                src.IdProducto,
                src.Producto.NombreProducto,
                src.Cantidad,
                src.PrecioUnitario,
                src.Subtotal
            ));
        CreateMap<CrearDetalleCompraDto, DetalleCompra>()
            .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.Cantidad * src.PrecioUnitario));

        // Devolucion mappings
        CreateMap<Devolucion, DevolucionDto>()
            .ConstructUsing(src => new DevolucionDto(
                src.IdDevolucion,
                src.IdVenta,
                src.IdProducto,
                src.Producto.NombreProducto,
                src.IdUsuario,
                src.Usuario.NombreCompleto,
                src.Cantidad,
                src.Motivo,
                src.FechaDevolucion,
                src.MontoDevuelto
            ));
        CreateMap<CrearDevolucionDto, Devolucion>();

        // Merma mappings
        CreateMap<Merma, MermaDto>()
            .ConstructUsing(src => new MermaDto(
                src.IdMerma,
                src.IdTienda,
                src.IdProducto,
                src.Producto.NombreProducto,
                src.IdUsuario,
                src.Usuario.NombreCompleto,
                src.Cantidad,
                src.Motivo,
                src.Descripcion,
                src.FechaMerma
            ));
        CreateMap<CrearMermaDto, Merma>();
    }
}
