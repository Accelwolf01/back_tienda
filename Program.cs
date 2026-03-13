using Microsoft.EntityFrameworkCore;
using Npgsql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using back_tienda.Infrastructure.Data;
using back_tienda.Infrastructure.Repositories;
using back_tienda.Application.Services;
using back_tienda.Application.Mappings;
using back_tienda.Application.Validators;
using back_tienda.Core.Interfaces;
using back_tienda.Core.Enums;

var builder = WebApplication.CreateBuilder(args);

// Configurar el puerto para Render (solo si existe la variable PORT)
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Limitar pool de conexiones a máximo 2 (plan gratuito de BD)
// Usamos Pooling=false para forzar el cierre inmediato y físico de cada conexión
var builderConn = new NpgsqlConnectionStringBuilder(connectionString)
{
    Pooling = false,
    Timeout = 20,
    CommandTimeout = 20
};

var dataSourceBuilder = new NpgsqlDataSourceBuilder(builderConn.ToString());

var dataSource = dataSourceBuilder.Build();
builder.Services.AddSingleton(dataSource);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(dataSource, npgsqlOptions => 
    {
        npgsqlOptions.CommandTimeout(20);
    });
    // Desactivar tracking por defecto para ahorrar memoria y liberar conexiones rápido
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

// Configurar AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

// Configurar FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<LoginValidator>();

// Registrar Unit of Work y Repositorios
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Registrar Servicios
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<ITiendaService, TiendaService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IProveedorService, ProveedorService>();
builder.Services.AddScoped<IVentaService, VentaService>();
builder.Services.AddScoped<ICompraService, CompraService>();
builder.Services.AddScoped<IDevolucionService, DevolucionService>();
builder.Services.AddScoped<IMermaService, MermaService>();
builder.Services.AddScoped<IReporteService, ReporteService>();

// Configurar JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey no configurada");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",
                "https://front-tienda-zudf.onrender.com"
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configurar Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// Configurar Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TiendaDB API",
        Version = "v1",
        Description = "API para Sistema de Inventario Multi-Tienda"
    });

    // Configurar JWT en Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Manejo de errores detallado (útil para debug en Render)
app.Use(async (context, next) =>
{
    try { await next(); }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        // Asegurar que CORS se envíe incluso en error
        context.Response.Headers.Append("Access-Control-Allow-Origin", "https://front-tienda-zudf.onrender.com");
        context.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
        
        await context.Response.WriteAsJsonAsync(new { 
            error = "Error interno", 
            message = ex.Message,
            detail = ex.InnerException?.Message 
        });
    }
});

// Configurar pipeline HTTP
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TiendaDB API v1");
    c.RoutePrefix = string.Empty; // Swagger en la raíz
});

// NOTA: No usar UseHttpsRedirection en Render (rompe CORS preflight).
// Render maneja HTTPS a nivel de proxy; el app recibe HTTP internamente.
// app.UseHttpsRedirection();

// CORS debe ir ANTES de Authentication y Authorization
app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
