using Arquitectura.Application.DTOs.Administracion;
using Arquitectura.Application.Interfaces.Administracion;
using Arquitectura.Domain.Entities;
using Arquitectura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Arquitectura.Application.Services.Administracion;

public class RolService : IRolService
{
    private readonly ArquitecturaDbContext _context;

    public RolService(ArquitecturaDbContext context)
    {
        _context = context;
    }

    public async Task<List<RolDto>> GetAllAsync()
    {
        return await _context.Roles
            .Select(r => new RolDto { Id = r.Id, Nombre = r.Nombre })
            .ToListAsync();
    }

    public async Task<RolDto?> GetByIdAsync(int id)
    {
        var rol = await _context.Roles.FindAsync(id);
        if (rol == null) return null;
        return new RolDto { Id = rol.Id, Nombre = rol.Nombre };
    }

    public async Task<RolDto> CreateAsync(CrearRolDto dto)
    {
        var rol = new Roles { Nombre = dto.Nombre };
        _context.Roles.Add(rol);
        await _context.SaveChangesAsync();
        return new RolDto { Id = rol.Id, Nombre = rol.Nombre };
    }

    public async Task<RolDto?> UpdateAsync(int id, ActualizarRolDto dto)
    {
        var rol = await _context.Roles.FindAsync(id);
        if (rol == null) return null;
        rol.Nombre = dto.Nombre;
        await _context.SaveChangesAsync();
        return new RolDto { Id = rol.Id, Nombre = rol.Nombre };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var rol = await _context.Roles.FindAsync(id);
        if (rol == null) return false;
        _context.Roles.Remove(rol);
        await _context.SaveChangesAsync();
        return true;
    }
}