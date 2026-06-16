using Arquitectura.Application.DTOs.Administracion;
using Arquitectura.Application.Interfaces.Administracion;
using Arquitectura.Application.DTOs.Auth;
using Arquitectura.Domain.Entities;
using Arquitectura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Arquitectura.Application.Services.Administracion;

public class UsuarioService : IUsuarioService
{
    private readonly ArquitecturaDbContext _context;

    public UsuarioService(ArquitecturaDbContext context)
    {
        _context = context;
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }

    private static UsuarioDto MapToDto(Usuario u) => new()
    {
        Id = u.Id,
        Nombre = u.Nombre,
        Apellidos = u.Apellidos,
        Email = u.Email,
        Telefono = u.Telefono,
        Puesto = u.Puesto,
        Salario = u.Salario,
        FechaContratacion = u.FechaContratacion,
        Estado = u.Estado,
        Admin = u.Admin,
        Roles = u.UserRoles.Select(ur => ur.Roles.Nombre).ToList()
    };

    public async Task<List<UsuarioDto>> GetAllAsync()
    {
        var usuarios = await _context.Usuario
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Roles)
            .Where(u => u.Estado != "Baja")
            .ToListAsync();
        return usuarios.Select(MapToDto).ToList();
    }

    public async Task<UsuarioDto?> GetByIdAsync(int id)
    {
        var u = await _context.Usuario
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Roles)
            .FirstOrDefaultAsync(u => u.Id == id);
        return u == null ? null : MapToDto(u);
    }

    public async Task<List<UsuarioDto>> BuscarAsync(string termino)
    {
        var t = termino.ToLower();
        var lista = await _context.Usuario
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Roles)
            .Where(u => u.Estado != "Baja" &&
                       (u.Nombre.ToLower().Contains(t) ||
                        u.Apellidos.ToLower().Contains(t) ||
                        u.Email.ToLower().Contains(t)))
            .ToListAsync();
        return lista.Select(MapToDto).ToList();
    }

    public async Task<UsuarioDto> CreateAsync(CrearUsuarioDto dto)
    {
        var usuario = new Usuario
        {
            Nombre = dto.Nombre,
            Apellidos = dto.Apellidos,
            Email = dto.Email,
            Telefono = dto.Telefono,
            Puesto = dto.Puesto,
            Salario = dto.Salario,
            FechaContratacion = dto.FechaContratacion,
            Admin = dto.Admin,
            Estado = "Activo",
            PasswordHash = HashPassword(dto.Password),
            FechaCreacion = DateTime.Now
        };

        _context.Usuario.Add(usuario);
        await _context.SaveChangesAsync();

        foreach (var rolId in dto.RolesIds)
        {
            _context.UserRoles.Add(new UserRoles { UserId = usuario.Id, RolesId = rolId });
        }
        await _context.SaveChangesAsync();

        return await GetByIdAsync(usuario.Id) ?? MapToDto(usuario);
    }

    public async Task<UsuarioDto?> UpdateAsync(int id, ActualizarUsuarioDto dto)
    {
        var usuario = await _context.Usuario.FindAsync(id);
        if (usuario == null) return null;

        usuario.Nombre = dto.Nombre;
        usuario.Apellidos = dto.Apellidos;
        usuario.Telefono = dto.Telefono;
        usuario.Puesto = dto.Puesto;
        usuario.Salario = dto.Salario;
        usuario.FechaContratacion = dto.FechaContratacion;
        usuario.Estado = dto.Estado;
        usuario.Admin = dto.Admin;

        await _context.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var usuario = await _context.Usuario.FindAsync(id);
        if (usuario == null) return false;
        usuario.Estado = "Baja";
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AsignarRolAsync(AsignarRolDto dto)
    {
        var existe = await _context.UserRoles
            .AnyAsync(ur => ur.UserId == dto.UsuarioId && ur.RolesId == dto.RolId);
        if (existe) return false;

        _context.UserRoles.Add(new UserRoles { UserId = dto.UsuarioId, RolesId = dto.RolId });
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoverRolAsync(int usuarioId, int rolId)
    {
        var ur = await _context.UserRoles
            .FirstOrDefaultAsync(x => x.UserId == usuarioId && x.RolesId == rolId);
        if (ur == null) return false;

        _context.UserRoles.Remove(ur);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<UsuarioDto?> LoginAsync(LoginDto dto)
{
    var usuario = await _context.Usuario
        .Include(u => u.UserRoles)
        .ThenInclude(ur => ur.Roles)
        .FirstOrDefaultAsync(u => u.Email == dto.Email);

    if (usuario == null)
        return null;

    if (usuario.PasswordHash != HashPassword(dto.Password))
        return null;

    return MapToDto(usuario);
}
}
