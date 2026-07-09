using Arquitectura.Application.Interfaces.Administracion;
using Arquitectura.Application.Interfaces.Empleados;
using Arquitectura.Application.Services.Administracion;
using Arquitectura.Application.Services.Empleados;
using Arquitectura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Arquitectura.API.Middlewares;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using Arquitectura.Application.Interfaces.Contabilidad;
using Arquitectura.Application.Services.Contabilidad;
using Arquitectura.Application.Interfaces.Seguimiento;
using Arquitectura.Application.Services.Seguimiento;
using Arquitectura.Application.Interfaces.Auth;
using Arquitectura.Application.Services.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.AddScoped<IContabilidadService, ContabilidadService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// ── Base de datos ─────────────────────────────────────────────
builder.Services.AddDbContext<ArquitecturaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Servicios - Módulo Administración ─────────────────────────
builder.Services.AddScoped<IRolService, RolService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

// ── Servicios - Módulo Empleados ──────────────────────────────
builder.Services.AddScoped<IEmpleadoService, EmpleadoService>();

// ── Servicios - Módulo Seguimiento ────────────────────────────
builder.Services.AddScoped<IProyectoService, ProyectoService>();
builder.Services.AddScoped<IComentarioProyectoService, ComentarioProyectoService>();
builder.Services.AddScoped<ITareaService, TareaService>();


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
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

app.UseCors("AllowAll");
app.UseResponseCompression();
app.UseMiddleware<ResponseTimeMiddleware>();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Arquitectura API v1");
    c.RoutePrefix = "swagger"; // antes era string.Empty
}); 


app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();