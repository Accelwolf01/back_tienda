namespace back_tienda.Core.Enums;

public enum TipoRol
{
    ADMIN_SISTEMA,
    DUEÑO_TIENDA,
    EMPLEADO
}

public enum EstadoUsuario
{
    ACTIVO,
    INACTIVO,
    BLOQUEADO
}

public enum TipoMovimiento
{
    COMPRA,
    VENTA,
    MERMA,
    DEVOLUCION,
    AJUSTE
}

public enum EstadoDocumento
{
    ACTIVO,
    CANCELADO,
    EDITADO
}
