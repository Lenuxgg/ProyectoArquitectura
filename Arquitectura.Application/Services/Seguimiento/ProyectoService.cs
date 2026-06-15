using Arquitectura.Application.DTOs.Seguimiento;
using Arquitectura.Application.Interfaces.Seguimiento;
using Arquitectura.Domain.Entities;
using Arquitectura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Arquitectura.Application.Services.Seguimiento;

public class ProyectoService : IProyectoService
{
    private readonly ArquitecturaDbContext _context;

    public ProyectoService(ArquitecturaDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProyectoDto>> ObtenerTodosAsync()
    {
        return await _context.Proyectos
            .Select(p => new ProyectoDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                FechaInicio = p.FechaInicio,
                FechaFin = p.FechaFin,
                Estado = p.Estado
            })
            .ToListAsync();
    }

    public async Task<ProyectoDto?> ObtenerPorIdAsync(int id)
    {
        return await _context.Proyectos
            .Where(p => p.Id == id)
            .Select(p => new ProyectoDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                FechaInicio = p.FechaInicio,
                FechaFin = p.FechaFin,
                Estado = p.Estado
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ProyectoDto> CrearAsync(CrearProyectoDto dto)
    {
        var proyecto = new Proyecto
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            FechaInicio = dto.FechaInicio,
            Estado = "Activo",
            FechaCreacion = DateTime.Now
        };

        _context.Proyectos.Add(proyecto);
        await _context.SaveChangesAsync();

        return new ProyectoDto
        {
            Id = proyecto.Id,
            Nombre = proyecto.Nombre,
            Descripcion = proyecto.Descripcion,
            FechaInicio = proyecto.FechaInicio,
            FechaFin = proyecto.FechaFin,
            Estado = proyecto.Estado
        };
    }

    public async Task<ProyectoDto?> EditarAsync(int id, EditarProyectoDto dto)
    {
        var proyecto = await _context.Proyectos.FindAsync(id);

        if (proyecto == null)
            return null;

        proyecto.Nombre = dto.Nombre;
        proyecto.Descripcion = dto.Descripcion;
        proyecto.FechaInicio = dto.FechaInicio;
        proyecto.FechaFin = dto.FechaFin;
        proyecto.Estado = dto.Estado;
        proyecto.FechaModificacion = DateTime.Now;

        await _context.SaveChangesAsync();

        return new ProyectoDto
        {
            Id = proyecto.Id,
            Nombre = proyecto.Nombre,
            Descripcion = proyecto.Descripcion,
            FechaInicio = proyecto.FechaInicio,
            FechaFin = proyecto.FechaFin,
            Estado = proyecto.Estado
        };
    }

    public async Task<bool> TerminarProyectoAsync(int id)
    {
        var proyecto = await _context.Proyectos.FindAsync(id);

        if (proyecto == null)
            return false;

        proyecto.Estado = "Terminado";
        proyecto.FechaFin = DateTime.Now;
        proyecto.FechaModificacion = DateTime.Now;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var proyecto = await _context.Proyectos.FindAsync(id);

        if (proyecto == null)
            return false;

        _context.Proyectos.Remove(proyecto);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> AsignarEmpleadoAsync(AsignarEmpleadoProyectoDto dto)
    {
        var proyectoExiste = await _context.Proyectos
            .AnyAsync(p => p.Id == dto.ProyectoId);

        var usuarioExiste = await _context.Usuario
            .AnyAsync(u => u.Id == dto.UsuarioId);

        if (!proyectoExiste || !usuarioExiste)
            return false;

        var yaExiste = await _context.ProyectoEmpleados
            .AnyAsync(x => x.ProyectoId == dto.ProyectoId &&
                           x.UsuarioId == dto.UsuarioId &&
                           x.Activo);

        if (yaExiste)
            return false;

        var asignacion = new ProyectoEmpleado
        {
            ProyectoId = dto.ProyectoId,
            UsuarioId = dto.UsuarioId,
            RolProyecto = dto.RolProyecto,
            FechaAsignacion = DateTime.Now,
            Activo = true
        };

        _context.ProyectoEmpleados.Add(asignacion);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> EditarAsignacionEmpleadoAsync(int asignacionId, EditarAsignacionProyectoDto dto)
    {
        var asignacion = await _context.ProyectoEmpleados.FindAsync(asignacionId);

        if (asignacion == null)
            return false;

        var usuarioExiste = await _context.Usuario
            .AnyAsync(u => u.Id == dto.UsuarioId);

        if (!usuarioExiste)
            return false;

        asignacion.UsuarioId = dto.UsuarioId;
        asignacion.RolProyecto = dto.RolProyecto;
        asignacion.Activo = dto.Activo;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> EliminarEmpleadoProyectoAsync(int asignacionId)
    {
        var asignacion = await _context.ProyectoEmpleados.FindAsync(asignacionId);

        if (asignacion == null)
            return false;

        _context.ProyectoEmpleados.Remove(asignacion);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<ProyectoEmpleadoDto>> ObtenerEmpleadosPorProyectoAsync(int proyectoId)
    {
        return await _context.ProyectoEmpleados
            .Include(x => x.Usuario)
            .Where(x => x.ProyectoId == proyectoId)
            .Select(x => new ProyectoEmpleadoDto
            {
                Id = x.Id,
                ProyectoId = x.ProyectoId,
                UsuarioId = x.UsuarioId,
                NombreEmpleado = x.Usuario.Nombre + " " + x.Usuario.Apellidos,
                RolProyecto = x.RolProyecto,
                FechaAsignacion = x.FechaAsignacion,
                Activo = x.Activo
            })
            .ToListAsync();
    }
}