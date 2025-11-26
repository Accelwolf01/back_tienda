using back_tienda.Core.Enums;

namespace back_tienda.Core.Entities;

public class Usuario
{
    public Guid IdUsuario { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string UsuarioNombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string ContraseñaHash { get; set; } = string.Empty;
    public TipoRol Rol { get; set; }
    public EstadoUsuario Estado { get; set; } = EstadoUsuario.ACTIVO;
    public int IntentosFallidos { get; set; } = 0;
    public DateTime? BloqueadoHasta { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaUltimaModificacion { get; set; } = DateTime.UtcNow;
    public Guid? CreadoPor { get; set; }

    // Navegación
    public virtual ICollection<Tienda> TiendasComoDueño { get; set; } = new List<Tienda>();
    public virtual ICollection<EmpleadoTienda> EmpleadoTiendas { get; set; } = new List<EmpleadoTienda>();
    public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    public virtual ICollection<Compra> Compras { get; set; } = new List<Compra>();
}
