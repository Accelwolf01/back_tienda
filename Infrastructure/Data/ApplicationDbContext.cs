using Microsoft.EntityFrameworkCore;
using back_tienda.Core.Entities;
using back_tienda.Core.Enums;

namespace back_tienda.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // DbSets
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Tienda> Tiendas => Set<Tienda>();
    public DbSet<EmpleadoTienda> EmpleadoTiendas => Set<EmpleadoTienda>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetalleVentas => Set<DetalleVenta>();
    public DbSet<Compra> Compras => Set<Compra>();
    public DbSet<DetalleCompra> DetalleCompras => Set<DetalleCompra>();
    public DbSet<Devolucion> Devoluciones => Set<Devolucion>();
    public DbSet<Merma> Mermas => Set<Merma>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("public");

        // Configurar entidades (todos los enums se guardan como texto)
        ConfigurarUsuario(modelBuilder);
        ConfigurarTienda(modelBuilder);
        ConfigurarEmpleadoTienda(modelBuilder);
        ConfigurarCategoria(modelBuilder);
        ConfigurarProveedor(modelBuilder);
        ConfigurarProducto(modelBuilder);
        ConfigurarVenta(modelBuilder);
        ConfigurarCompra(modelBuilder);
        ConfigurarDevolucion(modelBuilder);
        ConfigurarMerma(modelBuilder);
    }

    private void ConfigurarUsuario(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuarios");
            entity.HasKey(e => e.IdUsuario);
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.NombreCompleto).HasColumnName("nombre_completo").HasMaxLength(200).IsRequired();
            entity.Property(e => e.UsuarioNombre).HasColumnName("usuario").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Correo).HasColumnName("correo").HasMaxLength(150).IsRequired();
            entity.Property(e => e.ContraseñaHash).HasColumnName("contraseña_hash").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Rol).HasColumnName("rol").HasConversion<string>().IsRequired();
            entity.Property(e => e.Estado).HasColumnName("estado").HasConversion<string>().HasDefaultValue(EstadoUsuario.ACTIVO);
            entity.Property(e => e.IntentosFallidos).HasColumnName("intentos_fallidos").HasDefaultValue(0);
            entity.Property(e => e.BloqueadoHasta).HasColumnName("bloqueado_hasta");
            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.FechaUltimaModificacion).HasColumnName("fecha_ultima_modificacion").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.CreadoPor).HasColumnName("creado_por");

            entity.HasIndex(e => e.UsuarioNombre).IsUnique();
            entity.HasIndex(e => e.Correo).IsUnique();
        });
    }

    private void ConfigurarTienda(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tienda>(entity =>
        {
            entity.ToTable("tiendas");
            entity.HasKey(e => e.IdTienda);
            entity.Property(e => e.IdTienda).HasColumnName("id_tienda").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.NombreTienda).HasColumnName("nombre_tienda").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Direccion).HasColumnName("direccion");
            entity.Property(e => e.Telefono).HasColumnName("telefono").HasMaxLength(20);
            entity.Property(e => e.CorreoTienda).HasColumnName("correo_tienda").HasMaxLength(150);
            entity.Property(e => e.Nit).HasColumnName("nit").HasMaxLength(50);
            entity.Property(e => e.IdDueño).HasColumnName("id_dueño").IsRequired();
            entity.Property(e => e.Estado).HasColumnName("estado").HasConversion<string>().HasDefaultValue(EstadoUsuario.ACTIVO);
            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.FechaUltimaModificacion).HasColumnName("fecha_ultima_modificacion").HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.Dueño)
                .WithMany(u => u.TiendasComoDueño)
                .HasForeignKey(e => e.IdDueño)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigurarEmpleadoTienda(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EmpleadoTienda>(entity =>
        {
            entity.ToTable("empleados_tiendas");
            entity.HasKey(e => e.IdEmpleadoTienda);
            entity.Property(e => e.IdEmpleadoTienda).HasColumnName("id_empleado_tienda").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado").IsRequired();
            entity.Property(e => e.IdTienda).HasColumnName("id_tienda").IsRequired();
            entity.Property(e => e.PuedeVerReportes).HasColumnName("puede_ver_reportes").HasDefaultValue(false);
            entity.Property(e => e.PuedeGestionarInventario).HasColumnName("puede_gestionar_inventario").HasDefaultValue(true);
            entity.Property(e => e.PuedeRegistrarVentas).HasColumnName("puede_registrar_ventas").HasDefaultValue(true);
            entity.Property(e => e.PuedeRegistrarCompras).HasColumnName("puede_registrar_compras").HasDefaultValue(false);
            entity.Property(e => e.FechaAsignacion).HasColumnName("fecha_asignacion").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Estado).HasColumnName("estado").HasConversion<string>().HasDefaultValue(EstadoUsuario.ACTIVO);

            entity.HasOne(e => e.Empleado)
                .WithMany(u => u.EmpleadoTiendas)
                .HasForeignKey(e => e.IdEmpleado)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Tienda)
                .WithMany()
                .HasForeignKey(e => e.IdTienda)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.IdEmpleado, e.IdTienda }).IsUnique();
        });
    }

    private void ConfigurarCategoria(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.ToTable("categorias");
            entity.HasKey(e => e.IdCategoria);
            entity.Property(e => e.IdCategoria).HasColumnName("id_categoria").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.IdTienda).HasColumnName("id_tienda").IsRequired();
            entity.Property(e => e.NombreCategoria).HasColumnName("nombre_categoria").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion").HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.Tienda)
                .WithMany(t => t.Categorias)
                .HasForeignKey(e => e.IdTienda)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.IdTienda, e.NombreCategoria }).IsUnique();
        });
    }

    private void ConfigurarProveedor(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Proveedor>(entity =>
        {
            entity.ToTable("proveedores");
            entity.HasKey(e => e.IdProveedor);
            entity.Property(e => e.IdProveedor).HasColumnName("id_proveedor").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.IdTienda).HasColumnName("id_tienda").IsRequired();
            entity.Property(e => e.NombreProveedor).HasColumnName("nombre_proveedor").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Nit).HasColumnName("nit").HasMaxLength(50);
            entity.Property(e => e.Telefono).HasColumnName("telefono").HasMaxLength(20);
            entity.Property(e => e.Correo).HasColumnName("correo").HasMaxLength(150);
            entity.Property(e => e.Direccion).HasColumnName("direccion");
            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Estado).HasColumnName("estado").HasConversion<string>().HasDefaultValue(EstadoUsuario.ACTIVO);

            entity.HasOne(e => e.Tienda)
                .WithMany(t => t.Proveedores)
                .HasForeignKey(e => e.IdTienda)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.IdTienda, e.NombreProveedor }).IsUnique();
        });
    }

    private void ConfigurarProducto(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Producto>(entity =>
        {
            entity.ToTable("productos");
            entity.HasKey(e => e.IdProducto);
            entity.Property(e => e.IdProducto).HasColumnName("id_producto").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.IdTienda).HasColumnName("id_tienda").IsRequired();
            entity.Property(e => e.IdCategoria).HasColumnName("id_categoria");
            entity.Property(e => e.CodigoProducto).HasColumnName("codigo_producto").HasMaxLength(100);
            entity.Property(e => e.NombreProducto).HasColumnName("nombre_producto").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.PrecioVenta).HasColumnName("precio_venta").HasPrecision(12, 2).IsRequired();
            entity.Property(e => e.PrecioCompra).HasColumnName("precio_compra").HasPrecision(12, 2);
            entity.Property(e => e.StockActual).HasColumnName("stock_actual").HasDefaultValue(0);
            entity.Property(e => e.StockMinimo).HasColumnName("stock_minimo").HasDefaultValue(0);
            entity.Property(e => e.UnidadMedida).HasColumnName("unidad_medida").HasMaxLength(50).HasDefaultValue("UNIDAD");
            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.FechaUltimaModificacion).HasColumnName("fecha_ultima_modificacion").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Estado).HasColumnName("estado").HasConversion<string>().HasDefaultValue(EstadoUsuario.ACTIVO);

            entity.HasOne(e => e.Tienda)
                .WithMany(t => t.Productos)
                .HasForeignKey(e => e.IdTienda)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(e => e.IdCategoria)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => new { e.IdTienda, e.CodigoProducto }).IsUnique();
        });
    }

    private void ConfigurarVenta(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Venta>(entity =>
        {
            entity.ToTable("ventas");
            entity.HasKey(e => e.IdVenta);
            entity.Property(e => e.IdVenta).HasColumnName("id_venta").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.IdTienda).HasColumnName("id_tienda").IsRequired();
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario").IsRequired();
            entity.Property(e => e.FechaVenta).HasColumnName("fecha_venta").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.TotalVenta).HasColumnName("total_venta").HasPrecision(12, 2).HasDefaultValue(0);
            entity.Property(e => e.MetodoPago).HasColumnName("metodo_pago").HasMaxLength(50);
            entity.Property(e => e.Observaciones).HasColumnName("observaciones");
            entity.Property(e => e.Estado).HasColumnName("estado").HasConversion<string>().HasDefaultValue(EstadoDocumento.ACTIVO);

            entity.HasOne(e => e.Tienda)
                .WithMany(t => t.Ventas)
                .HasForeignKey(e => e.IdTienda)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Usuario)
                .WithMany(u => u.Ventas)
                .HasForeignKey(e => e.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DetalleVenta>(entity =>
        {
            entity.ToTable("detalle_ventas");
            entity.HasKey(e => e.IdDetalleVenta);
            entity.Property(e => e.IdDetalleVenta).HasColumnName("id_detalle_venta").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.IdVenta).HasColumnName("id_venta").IsRequired();
            entity.Property(e => e.IdProducto).HasColumnName("id_producto").IsRequired();
            entity.Property(e => e.Cantidad).HasColumnName("cantidad").IsRequired();
            entity.Property(e => e.PrecioUnitario).HasColumnName("precio_unitario").HasPrecision(12, 2).IsRequired();
            entity.Property(e => e.Subtotal).HasColumnName("subtotal").HasPrecision(12, 2).IsRequired();

            entity.HasOne(e => e.Venta)
                .WithMany(v => v.DetalleVentas)
                .HasForeignKey(e => e.IdVenta)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Producto)
                .WithMany(p => p.DetalleVentas)
                .HasForeignKey(e => e.IdProducto)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigurarCompra(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Compra>(entity =>
        {
            entity.ToTable("compras");
            entity.HasKey(e => e.IdCompra);
            entity.Property(e => e.IdCompra).HasColumnName("id_compra").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.IdTienda).HasColumnName("id_tienda").IsRequired();
            entity.Property(e => e.IdProveedor).HasColumnName("id_proveedor");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario").IsRequired();
            entity.Property(e => e.NumeroFactura).HasColumnName("numero_factura").HasMaxLength(100);
            entity.Property(e => e.FechaCompra).HasColumnName("fecha_compra").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.TotalCompra).HasColumnName("total_compra").HasPrecision(12, 2).HasDefaultValue(0);
            entity.Property(e => e.Observaciones).HasColumnName("observaciones");
            entity.Property(e => e.Estado).HasColumnName("estado").HasConversion<string>().HasDefaultValue(EstadoDocumento.ACTIVO);
            entity.Property(e => e.PuedeEditar).HasColumnName("puede_editar").HasDefaultValue(true);
            entity.Property(e => e.FechaLimiteEdicion).HasColumnName("fecha_limite_edicion");

            entity.HasOne(e => e.Tienda)
                .WithMany(t => t.Compras)
                .HasForeignKey(e => e.IdTienda)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Proveedor)
                .WithMany(p => p.Compras)
                .HasForeignKey(e => e.IdProveedor)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Usuario)
                .WithMany(u => u.Compras)
                .HasForeignKey(e => e.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DetalleCompra>(entity =>
        {
            entity.ToTable("detalle_compras");
            entity.HasKey(e => e.IdDetalleCompra);
            entity.Property(e => e.IdDetalleCompra).HasColumnName("id_detalle_compra").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.IdCompra).HasColumnName("id_compra").IsRequired();
            entity.Property(e => e.IdProducto).HasColumnName("id_producto").IsRequired();
            entity.Property(e => e.Cantidad).HasColumnName("cantidad").IsRequired();
            entity.Property(e => e.PrecioUnitario).HasColumnName("precio_unitario").HasPrecision(12, 2).IsRequired();
            entity.Property(e => e.Subtotal).HasColumnName("subtotal").HasPrecision(12, 2).IsRequired();

            entity.HasOne(e => e.Compra)
                .WithMany(c => c.DetalleCompras)
                .HasForeignKey(e => e.IdCompra)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Producto)
                .WithMany(p => p.DetalleCompras)
                .HasForeignKey(e => e.IdProducto)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigurarDevolucion(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Devolucion>(entity =>
        {
            entity.ToTable("devoluciones");
            entity.HasKey(e => e.IdDevolucion);
            entity.Property(e => e.IdDevolucion).HasColumnName("id_devolucion").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.IdVenta).HasColumnName("id_venta").IsRequired();
            entity.Property(e => e.IdProducto).HasColumnName("id_producto").IsRequired();
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario").IsRequired();
            entity.Property(e => e.Cantidad).HasColumnName("cantidad").IsRequired();
            entity.Property(e => e.Motivo).HasColumnName("motivo");
            entity.Property(e => e.FechaDevolucion).HasColumnName("fecha_devolucion").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.MontoDevuelto).HasColumnName("monto_devuelto").HasPrecision(12, 2);
        });
    }

    private void ConfigurarMerma(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Merma>(entity =>
        {
            entity.ToTable("mermas");
            entity.HasKey(e => e.IdMerma);
            entity.Property(e => e.IdMerma).HasColumnName("id_merma").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.IdTienda).HasColumnName("id_tienda").IsRequired();
            entity.Property(e => e.IdProducto).HasColumnName("id_producto").IsRequired();
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario").IsRequired();
            entity.Property(e => e.Cantidad).HasColumnName("cantidad").IsRequired();
            entity.Property(e => e.Motivo).HasColumnName("motivo").HasMaxLength(100);
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.FechaMerma).HasColumnName("fecha_merma").HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}
