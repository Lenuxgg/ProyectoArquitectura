using System.IO.Compression;
using System.Text;
using Arquitectura.API.Middlewares;
using Arquitectura.Application.Interfaces.Administracion;
using Arquitectura.Application.Interfaces.Auth;
using Arquitectura.Application.Interfaces.Contabilidad;
using Arquitectura.Application.Interfaces.Empleados;
using Arquitectura.Application.Interfaces.Exportaciones;
using Arquitectura.Application.Interfaces.Notificaciones;
using Arquitectura.Application.Interfaces.Seguimiento;
using Arquitectura.Application.Services.Administracion;
using Arquitectura.Application.Services.Auth;
using Arquitectura.Application.Services.Contabilidad;
using Arquitectura.Application.Services.Empleados;
using Arquitectura.Application.Services.Exportaciones;
using Arquitectura.Application.Services.Notificaciones;
using Arquitectura.Application.Services.Seguimiento;
using Arquitectura.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Arquitectura.Application.Interfaces.Cuenta;
using Arquitectura.Application.Services.Cuenta;

var builder = WebApplication.CreateBuilder(args);

// ── RNF-002 - Compresión de respuestas ─────────────────────────
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

// ── Base de datos ─────────────────────────────────────────────
builder.Services.AddDbContext<ArquitecturaDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
        }));

// ── Servicios - Módulo Administración ─────────────────────────
builder.Services.AddScoped<IRolService, RolService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

// ── Servicios - Módulo Empleados ──────────────────────────────
builder.Services.AddScoped<IEmpleadoService, EmpleadoService>();

// ── Servicios - Módulo Seguimiento ────────────────────────────
builder.Services.AddScoped<IProyectoService, ProyectoService>();
builder.Services.AddScoped<IComentarioProyectoService, ComentarioProyectoService>();
builder.Services.AddScoped<ITareaService, TareaService>();

// ── Servicios - Módulo Contabilidad ───────────────────────────
builder.Services.AddScoped<IContabilidadService, ContabilidadService>();

// ── Servicios - Módulo Auth ───────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();

// ── Servicios - Módulo Exportaciones ──────────────────────────
builder.Services.AddScoped<IExportacionService, ExportacionService>();

// ── Servicios - Módulo Notificaciones ─────────────────────────
builder.Services.AddScoped<INotificacionService, NotificacionService>();

// ── Servicios - Módulo Cuenta ────────────────────────────────
builder.Services.AddScoped<ICuentaService, CuentaService>();

// ── JWT ───────────────────────────────────────────────────────
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

// ── Controllers + Swagger ─────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Arquitectura API",
        Version = "v1",
        Description = "API del Sistema de Administración General para Firma de Arquitectura"
    });
});

// ── CORS ──────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var app = builder.Build();

// ── Middlewares ───────────────────────────────────────────────
app.UseCors("AllowAll");

app.UseResponseCompression();

app.UseMiddleware<ResponseTimeMiddleware>();

app.UseDefaultFiles();

app.UseStaticFiles();

app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Arquitectura API v1");
    c.RoutePrefix = "swagger";
});

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();