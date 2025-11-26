using back_tienda.Core.Enums;

namespace back_tienda.Core.DTOs;

// DTOs de Autenticación
public record LoginRequestDto(string Usuario, string Contraseña);

public record LoginResponseDto(
    Guid IdUsuario,
    string NombreCompleto,
    string Usuario,
    string Correo,
    TipoRol Rol,
    string Token,
    DateTime Expiracion,
    Guid? IdTienda
);

public record RegistroUsuarioDto(
    string NombreCompleto,
    string Usuario,
    string Correo,
    string Contraseña,
    TipoRol Rol
);

// DTOs de Usuario
public record UsuarioDto(
    Guid IdUsuario,
    string NombreCompleto,
    string Usuario,
    string Correo,
    TipoRol Rol,
    EstadoUsuario Estado
)
{
    public Guid Id => IdUsuario;
};


public record ActualizarUsuarioDto(
    string? NombreCompleto,
    string? Correo,
    EstadoUsuario? Estado,
    TipoRol? Rol
);

public record CambiarContraseñaDto(
    string ContraseñaActual,
    string ContraseñaNueva
);

// DTOs de Tienda
public record TiendaDto(
    Guid IdTienda,
    string NombreTienda,
    string? Direccion,
    string? Telefono,
    string? CorreoTienda,
    string? Nit,
    Guid IdDueño,
    string NombreDueño,
    EstadoUsuario Estado
)
{
    public Guid Id => IdTienda;
};


public record CrearTiendaDto(
    string NombreTienda,
    string? Direccion,
    string? Telefono,
    string? CorreoTienda,
    string? Nit,
    Guid IdDueño
);

public record ActualizarTiendaDto(
    string? NombreTienda,
    string? Direccion,
    string? Telefono,
    string? CorreoTienda,
    string? Nit,
    EstadoUsuario? Estado
);

// DTOs de Producto
public record ProductoDto(
    Guid IdProducto,
    Guid IdTienda,
    Guid? IdCategoria,
    string? CodigoProducto,
    string NombreProducto,
    string? Descripcion,
    decimal PrecioVenta,
    decimal? PrecioCompra,
    int StockActual,
    int StockMinimo,
    string UnidadMedida,
    string? NombreCategoria,
    EstadoUsuario Estado
)
{
    public Guid Id => IdProducto;
};


public record CrearProductoDto(
    Guid? IdTienda,
    Guid? IdCategoria,
    string? CodigoProducto,
    string NombreProducto,
    string? Descripcion,
    decimal PrecioVenta,
    decimal? PrecioCompra,
    int StockActual,
    int StockMinimo,
    string UnidadMedida
);

public record ActualizarProductoDto(
    string? NombreProducto,
    string? Descripcion,
    decimal? PrecioVenta,
    decimal? PrecioCompra,
    int? StockMinimo,
    Guid? IdCategoria
);

// DTOs de Categoría
public record CategoriaDto(
    Guid IdCategoria,
    Guid IdTienda,
    string NombreCategoria,
    string? Descripcion
)
{
    public Guid Id => IdCategoria;
};


public record CrearCategoriaDto(
    Guid IdTienda,
    string NombreCategoria,
    string? Descripcion
);

// DTOs de Proveedor
public record ProveedorDto(
    Guid IdProveedor,
    Guid IdTienda,
    string NombreProveedor,
    string? Nit,
    string? Telefono,
    string? Correo,
    string? Direccion,
    EstadoUsuario Estado
)
{
    public Guid Id => IdProveedor;
};


public record CrearProveedorDto(
    Guid IdTienda,
    string NombreProveedor,
    string? Nit,
    string? Telefono,
    string? Correo,
    string? Direccion
);

public record ActualizarVentaDto(
    string? MetodoPago,
    string? Observaciones
);
