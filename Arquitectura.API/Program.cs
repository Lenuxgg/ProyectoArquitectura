using Arquitectura.Application.Interfaces.Administracion;
using Arquitectura.Application.Interfaces.Empleados;
using Arquitectura.Application.Services.Administracion;
using Arquitectura.Application.Services.Empleados;
using Arquitectura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Arquitectura.Application.Interfaces.Contabilidad;
using Arquitectura.Application.Services.Contabilidad;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IContabilidadService, ContabilidadService>();

// ── Base de datos ─────────────────────────────────────────────
builder.Services.AddDbContext<ArquitecturaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Servicios - Módulo Administración ─────────────────────────
builder.Services.AddScoped<IRolService, RolService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

// ── Servicios - Módulo Empleados ──────────────────────────────
builder.Services.AddScoped<IEmpleadoService, EmpleadoService>();

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
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Arquitectura API v1");
    c.RoutePrefix = string.Empty;
});

app.UseAuthorization();
app.MapControllers();
app.Run();