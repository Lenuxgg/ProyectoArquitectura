using Arquitectura.Application.DTOs.Empleados;
using Arquitectura.Application.Interfaces.Empleados;
using Arquitectura.Domain.Entities;
using Arquitectura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Arquitectura.Application.Services.Empleados;

public class EmpleadoService : IEmpleadoService
{
    private readonly ArquitecturaDbContext _context;

    public EmpleadoService(ArquitecturaDbContext context)
    {
        _context = context;
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }

    private static EmpleadoDto MapToDto(Usuario u) => new()
    {
        Id = u.Id,
        Nombre = u.Nombre,
        Apellidos = u.Apellidos,
        Email = u.Email,
        Telefono = u.Telefono,
        Puesto = u.Puesto,
        Salario = u.Salario,
        FechaContratacion = u.FechaContratacion,
        Estado = u.Estado
    };

    public async Task<List<EmpleadoDto>> GetAllAsync()
    {
        var empleados = await _context.Usuario
            .Where(u => u.Estado != "Baja" && u.Admin == false)
            .ToListAsync();
        return empleados.Select(MapToDto).ToList();
    }

    public async Task<EmpleadoDto?> GetByIdAsync(int id)
    {
        var u = await _context.Usuario
            .FirstOrDefaultAsync(u => u.Id == id && u.Admin == false);
        return u == null ? null : MapToDto(u);
    }

    public async Task<EmpleadoDto> CrearEmpleadoAsync(CrearEmpleadoDto dto)
    {
        var empleado = new Usuario
        {
            Nombre = dto.Nombre,
            Apellidos = dto.Apellidos,
            Email = dto.Email,
            Telefono = dto.Telefono,
            Puesto = dto.Puesto,
            Salario = dto.Salario,
            FechaContratacion = dto.FechaContratacion,
            Estado = "Activo",
            Admin = false,
            PasswordHash = HashPassword(dto.Password),
            FechaCreacion = DateTime.Now
        };

        _context.Usuario.Add(empleado);
        await _context.SaveChangesAsync();

        // Asignar rol Empleado (Id = 2) automáticamente
        _context.UserRoles.Add(new UserRoles { UserId = empleado.Id, RolesId = 2 });
        await _context.SaveChangesAsync();

        return MapToDto(empleado);
    }

    public async Task<EmpleadoDto?> ActualizarPuestoAsync(int id, ActualizarPuestoDto dto)
    {
        var empleado = await _context.Usuario
            .FirstOrDefaultAsync(u => u.Id == id && u.Admin == false);
        if (empleado == null) return null;

        empleado.Puesto = dto.Puesto;
        await _context.SaveChangesAsync();
        return MapToDto(empleado);
    }

    public async Task<EmpleadoDto?> ActualizarSalarioAsync(int id, ActualizarSalarioDto dto)
    {
        if (dto.Salario <= 0)
            throw new ArgumentException("El salario debe ser mayor a cero.");

        var empleado = await _context.Usuario
            .FirstOrDefaultAsync(u => u.Id == id && u.Admin == false);
        if (empleado == null) return null;

        empleado.Salario = dto.Salario;
        await _context.SaveChangesAsync();
        return MapToDto(empleado);
    }

    public async Task<bool> DarDeBajaAsync(int id)
    {
        var empleado = await _context.Usuario
            .FirstOrDefaultAsync(u => u.Id == id && u.Admin == false);
        if (empleado == null) return false;

        empleado.Estado = "Baja";
        await _context.SaveChangesAsync();
        return true;
    }
}