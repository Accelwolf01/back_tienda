using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace back_tienda.Migrations
{
    /// <inheritdoc />
    public partial class UsePostgresEnums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "usuarios",
                schema: "public",
                columns: table => new
                {
                    id_usuario = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    nombre_completo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    usuario = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    correo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    contraseña_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    rol = table.Column<int>(type: "tipo_rol", nullable: false),
                    estado = table.Column<int>(type: "estado_usuario", nullable: false, defaultValue: 0),
                    intentos_fallidos = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    bloqueado_hasta = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    fecha_ultima_modificacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    creado_por = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id_usuario);
                });

            migrationBuilder.CreateTable(
                name: "tiendas",
                schema: "public",
                columns: table => new
                {
                    id_tienda = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    nombre_tienda = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    direccion = table.Column<string>(type: "text", nullable: true),
                    telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    correo_tienda = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    nit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    id_dueño = table.Column<Guid>(type: "uuid", nullable: false),
                    estado = table.Column<int>(type: "estado_usuario", nullable: false, defaultValue: 0),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    fecha_ultima_modificacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tiendas", x => x.id_tienda);
                    table.ForeignKey(
                        name: "FK_tiendas_usuarios_id_dueño",
                        column: x => x.id_dueño,
                        principalSchema: "public",
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "categorias",
                schema: "public",
                columns: table => new
                {
                    id_categoria = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    id_tienda = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre_categoria = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categorias", x => x.id_categoria);
                    table.ForeignKey(
                        name: "FK_categorias_tiendas_id_tienda",
                        column: x => x.id_tienda,
                        principalSchema: "public",
                        principalTable: "tiendas",
                        principalColumn: "id_tienda",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "empleados_tiendas",
                schema: "public",
                columns: table => new
                {
                    id_empleado_tienda = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    id_empleado = table.Column<Guid>(type: "uuid", nullable: false),
                    id_tienda = table.Column<Guid>(type: "uuid", nullable: false),
                    puede_ver_reportes = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    puede_gestionar_inventario = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    puede_registrar_ventas = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    puede_registrar_compras = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    fecha_asignacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    estado = table.Column<int>(type: "estado_usuario", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_empleados_tiendas", x => x.id_empleado_tienda);
                    table.ForeignKey(
                        name: "FK_empleados_tiendas_tiendas_id_tienda",
                        column: x => x.id_tienda,
                        principalSchema: "public",
                        principalTable: "tiendas",
                        principalColumn: "id_tienda",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_empleados_tiendas_usuarios_id_empleado",
                        column: x => x.id_empleado,
                        principalSchema: "public",
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "proveedores",
                schema: "public",
                columns: table => new
                {
                    id_proveedor = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    id_tienda = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre_proveedor = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    nit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    correo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    direccion = table.Column<string>(type: "text", nullable: true),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    estado = table.Column<int>(type: "estado_usuario", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proveedores", x => x.id_proveedor);
                    table.ForeignKey(
                        name: "FK_proveedores_tiendas_id_tienda",
                        column: x => x.id_tienda,
                        principalSchema: "public",
                        principalTable: "tiendas",
                        principalColumn: "id_tienda",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ventas",
                schema: "public",
                columns: table => new
                {
                    id_venta = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    id_tienda = table.Column<Guid>(type: "uuid", nullable: false),
                    id_usuario = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha_venta = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    total_venta = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false, defaultValue: 0m),
                    metodo_pago = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    observaciones = table.Column<string>(type: "text", nullable: true),
                    estado = table.Column<int>(type: "estado_documento", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ventas", x => x.id_venta);
                    table.ForeignKey(
                        name: "FK_ventas_tiendas_id_tienda",
                        column: x => x.id_tienda,
                        principalSchema: "public",
                        principalTable: "tiendas",
                        principalColumn: "id_tienda",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ventas_usuarios_id_usuario",
                        column: x => x.id_usuario,
                        principalSchema: "public",
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "productos",
                schema: "public",
                columns: table => new
                {
                    id_producto = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    id_tienda = table.Column<Guid>(type: "uuid", nullable: false),
                    id_categoria = table.Column<Guid>(type: "uuid", nullable: true),
                    codigo_producto = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    nombre_producto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    precio_venta = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    precio_compra = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    stock_actual = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    stock_minimo = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    unidad_medida = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "UNIDAD"),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    fecha_ultima_modificacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    estado = table.Column<int>(type: "estado_usuario", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_productos", x => x.id_producto);
                    table.ForeignKey(
                        name: "FK_productos_categorias_id_categoria",
                        column: x => x.id_categoria,
                        principalSchema: "public",
                        principalTable: "categorias",
                        principalColumn: "id_categoria",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_productos_tiendas_id_tienda",
                        column: x => x.id_tienda,
                        principalSchema: "public",
                        principalTable: "tiendas",
                        principalColumn: "id_tienda",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compras",
                schema: "public",
                columns: table => new
                {
                    id_compra = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    id_tienda = table.Column<Guid>(type: "uuid", nullable: false),
                    id_proveedor = table.Column<Guid>(type: "uuid", nullable: true),
                    id_usuario = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_factura = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    fecha_compra = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    total_compra = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false, defaultValue: 0m),
                    observaciones = table.Column<string>(type: "text", nullable: true),
                    estado = table.Column<int>(type: "estado_documento", nullable: false, defaultValue: 0),
                    puede_editar = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    fecha_limite_edicion = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compras", x => x.id_compra);
                    table.ForeignKey(
                        name: "FK_compras_proveedores_id_proveedor",
                        column: x => x.id_proveedor,
                        principalSchema: "public",
                        principalTable: "proveedores",
                        principalColumn: "id_proveedor",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_compras_tiendas_id_tienda",
                        column: x => x.id_tienda,
                        principalSchema: "public",
                        principalTable: "tiendas",
                        principalColumn: "id_tienda",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_compras_usuarios_id_usuario",
                        column: x => x.id_usuario,
                        principalSchema: "public",
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "detalle_ventas",
                schema: "public",
                columns: table => new
                {
                    id_detalle_venta = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    id_venta = table.Column<Guid>(type: "uuid", nullable: false),
                    id_producto = table.Column<Guid>(type: "uuid", nullable: false),
                    cantidad = table.Column<int>(type: "integer", nullable: false),
                    precio_unitario = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_detalle_ventas", x => x.id_detalle_venta);
                    table.ForeignKey(
                        name: "FK_detalle_ventas_productos_id_producto",
                        column: x => x.id_producto,
                        principalSchema: "public",
                        principalTable: "productos",
                        principalColumn: "id_producto",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_detalle_ventas_ventas_id_venta",
                        column: x => x.id_venta,
                        principalSchema: "public",
                        principalTable: "ventas",
                        principalColumn: "id_venta",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "devoluciones",
                schema: "public",
                columns: table => new
                {
                    id_devolucion = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    id_venta = table.Column<Guid>(type: "uuid", nullable: false),
                    id_producto = table.Column<Guid>(type: "uuid", nullable: false),
                    id_usuario = table.Column<Guid>(type: "uuid", nullable: false),
                    cantidad = table.Column<int>(type: "integer", nullable: false),
                    motivo = table.Column<string>(type: "text", nullable: true),
                    fecha_devolucion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    monto_devuelto = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    VentaIdVenta = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductoIdProducto = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioIdUsuario = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_devoluciones", x => x.id_devolucion);
                    table.ForeignKey(
                        name: "FK_devoluciones_productos_ProductoIdProducto",
                        column: x => x.ProductoIdProducto,
                        principalSchema: "public",
                        principalTable: "productos",
                        principalColumn: "id_producto",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_devoluciones_usuarios_UsuarioIdUsuario",
                        column: x => x.UsuarioIdUsuario,
                        principalSchema: "public",
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_devoluciones_ventas_VentaIdVenta",
                        column: x => x.VentaIdVenta,
                        principalSchema: "public",
                        principalTable: "ventas",
                        principalColumn: "id_venta",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mermas",
                schema: "public",
                columns: table => new
                {
                    id_merma = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    id_tienda = table.Column<Guid>(type: "uuid", nullable: false),
                    id_producto = table.Column<Guid>(type: "uuid", nullable: false),
                    id_usuario = table.Column<Guid>(type: "uuid", nullable: false),
                    cantidad = table.Column<int>(type: "integer", nullable: false),
                    motivo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    fecha_merma = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    TiendaIdTienda = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductoIdProducto = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioIdUsuario = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mermas", x => x.id_merma);
                    table.ForeignKey(
                        name: "FK_mermas_productos_ProductoIdProducto",
                        column: x => x.ProductoIdProducto,
                        principalSchema: "public",
                        principalTable: "productos",
                        principalColumn: "id_producto",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_mermas_tiendas_TiendaIdTienda",
                        column: x => x.TiendaIdTienda,
                        principalSchema: "public",
                        principalTable: "tiendas",
                        principalColumn: "id_tienda",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_mermas_usuarios_UsuarioIdUsuario",
                        column: x => x.UsuarioIdUsuario,
                        principalSchema: "public",
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "detalle_compras",
                schema: "public",
                columns: table => new
                {
                    id_detalle_compra = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    id_compra = table.Column<Guid>(type: "uuid", nullable: false),
                    id_producto = table.Column<Guid>(type: "uuid", nullable: false),
                    cantidad = table.Column<int>(type: "integer", nullable: false),
                    precio_unitario = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_detalle_compras", x => x.id_detalle_compra);
                    table.ForeignKey(
                        name: "FK_detalle_compras_compras_id_compra",
                        column: x => x.id_compra,
                        principalSchema: "public",
                        principalTable: "compras",
                        principalColumn: "id_compra",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_detalle_compras_productos_id_producto",
                        column: x => x.id_producto,
                        principalSchema: "public",
                        principalTable: "productos",
                        principalColumn: "id_producto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_categorias_id_tienda_nombre_categoria",
                schema: "public",
                table: "categorias",
                columns: new[] { "id_tienda", "nombre_categoria" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compras_id_proveedor",
                schema: "public",
                table: "compras",
                column: "id_proveedor");

            migrationBuilder.CreateIndex(
                name: "IX_compras_id_tienda",
                schema: "public",
                table: "compras",
                column: "id_tienda");

            migrationBuilder.CreateIndex(
                name: "IX_compras_id_usuario",
                schema: "public",
                table: "compras",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_compras_id_compra",
                schema: "public",
                table: "detalle_compras",
                column: "id_compra");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_compras_id_producto",
                schema: "public",
                table: "detalle_compras",
                column: "id_producto");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_ventas_id_producto",
                schema: "public",
                table: "detalle_ventas",
                column: "id_producto");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_ventas_id_venta",
                schema: "public",
                table: "detalle_ventas",
                column: "id_venta");

            migrationBuilder.CreateIndex(
                name: "IX_devoluciones_ProductoIdProducto",
                schema: "public",
                table: "devoluciones",
                column: "ProductoIdProducto");

            migrationBuilder.CreateIndex(
                name: "IX_devoluciones_UsuarioIdUsuario",
                schema: "public",
                table: "devoluciones",
                column: "UsuarioIdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_devoluciones_VentaIdVenta",
                schema: "public",
                table: "devoluciones",
                column: "VentaIdVenta");

            migrationBuilder.CreateIndex(
                name: "IX_empleados_tiendas_id_empleado_id_tienda",
                schema: "public",
                table: "empleados_tiendas",
                columns: new[] { "id_empleado", "id_tienda" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_empleados_tiendas_id_tienda",
                schema: "public",
                table: "empleados_tiendas",
                column: "id_tienda");

            migrationBuilder.CreateIndex(
                name: "IX_mermas_ProductoIdProducto",
                schema: "public",
                table: "mermas",
                column: "ProductoIdProducto");

            migrationBuilder.CreateIndex(
                name: "IX_mermas_TiendaIdTienda",
                schema: "public",
                table: "mermas",
                column: "TiendaIdTienda");

            migrationBuilder.CreateIndex(
                name: "IX_mermas_UsuarioIdUsuario",
                schema: "public",
                table: "mermas",
                column: "UsuarioIdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_productos_id_categoria",
                schema: "public",
                table: "productos",
                column: "id_categoria");

            migrationBuilder.CreateIndex(
                name: "IX_productos_id_tienda_codigo_producto",
                schema: "public",
                table: "productos",
                columns: new[] { "id_tienda", "codigo_producto" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_proveedores_id_tienda_nombre_proveedor",
                schema: "public",
                table: "proveedores",
                columns: new[] { "id_tienda", "nombre_proveedor" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tiendas_id_dueño",
                schema: "public",
                table: "tiendas",
                column: "id_dueño");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_correo",
                schema: "public",
                table: "usuarios",
                column: "correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_usuario",
                schema: "public",
                table: "usuarios",
                column: "usuario",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ventas_id_tienda",
                schema: "public",
                table: "ventas",
                column: "id_tienda");

            migrationBuilder.CreateIndex(
                name: "IX_ventas_id_usuario",
                schema: "public",
                table: "ventas",
                column: "id_usuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "detalle_compras",
                schema: "public");

            migrationBuilder.DropTable(
                name: "detalle_ventas",
                schema: "public");

            migrationBuilder.DropTable(
                name: "devoluciones",
                schema: "public");

            migrationBuilder.DropTable(
                name: "empleados_tiendas",
                schema: "public");

            migrationBuilder.DropTable(
                name: "mermas",
                schema: "public");

            migrationBuilder.DropTable(
                name: "compras",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ventas",
                schema: "public");

            migrationBuilder.DropTable(
                name: "productos",
                schema: "public");

            migrationBuilder.DropTable(
                name: "proveedores",
                schema: "public");

            migrationBuilder.DropTable(
                name: "categorias",
                schema: "public");

            migrationBuilder.DropTable(
                name: "tiendas",
                schema: "public");

            migrationBuilder.DropTable(
                name: "usuarios",
                schema: "public");
        }
    }
}
