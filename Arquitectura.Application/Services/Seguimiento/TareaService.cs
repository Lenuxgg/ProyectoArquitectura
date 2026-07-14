using Arquitectura.Application.DTOs.Seguimiento;
using Arquitectura.Application.Interfaces.Seguimiento;
using Arquitectura.Domain.Entities;
using Arquitectura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Arquitectura.Application.DTOs.Notificaciones;
using Arquitectura.Application.Interfaces.Notificaciones;

namespace Arquitectura.Application.Services.Seguimiento;

public class TareaService : ITareaService
{
    private readonly ArquitecturaDbContext _context;
    private readonly INotificacionService _notificacionService;

    public TareaService(
        ArquitecturaDbContext context,
        INotificacionService notificacionService)
    {
        _context = context;
        _notificacionService = notificacionService;
    }

    public async Task<List<TareaDto>> ObtenerTodasAsync()
    {
        return await _context.Tareas
            .Select(t => new TareaDto
            {
                Id = t.Id,
                Titulo = t.Titulo,
                Estado = t.Estado,
                ProyectoId = t.ProyectoId,
                Descripcion = t.Descripcion,
                FechaInicio = t.FechaInicio,
                FechaFin = t.FechaFin
            })
            .ToListAsync();
    }

    public async Task<List<TareaDto>> ObtenerPorProyectoAsync(int proyectoId)
    {
        return await _context.Tareas
            .Where(t => t.ProyectoId == proyectoId)
            .Select(t => new TareaDto
            {
                Id = t.Id,
                Titulo = t.Titulo,
                Estado = t.Estado,
                ProyectoId = t.ProyectoId,
                Descripcion = t.Descripcion,
                FechaInicio = t.FechaInicio,
                FechaFin = t.FechaFin
            })
            .ToListAsync();
    }

    public async Task<TareaDto?> ObtenerPorIdAsync(int id)
    {
        return await _context.Tareas
            .Where(t => t.Id == id)
            .Select(t => new TareaDto
            {
                Id = t.Id,
                Titulo = t.Titulo,
                Estado = t.Estado,
                ProyectoId = t.ProyectoId,
                Descripcion = t.Descripcion,
                FechaInicio = t.FechaInicio,
                FechaFin = t.FechaFin
            })
            .FirstOrDefaultAsync();
    }

    public async Task<TareaDto> CrearAsync(CrearTareaDto dto)
    {
        var tarea = new Tarea
        {
            Titulo = dto.Titulo,
            ProyectoId = dto.ProyectoId,
            Descripcion = dto.Descripcion,
            FechaInicio = dto.FechaInicio,
            FechaFin = dto.FechaFin,
            Estado = "Pendiente",
            FechaCreacion = DateTime.Now
        };

        _context.Tareas.Add(tarea);
        await _context.SaveChangesAsync();

        return new TareaDto
        {
            Id = tarea.Id,
            Titulo = tarea.Titulo,
            Estado = tarea.Estado,
            ProyectoId = tarea.ProyectoId,
            Descripcion = tarea.Descripcion,
            FechaInicio = tarea.FechaInicio,
            FechaFin = tarea.FechaFin
        };
    }

    public async Task<TareaDto?> EditarAsync(int id, EditarTareaDto dto)
    {
        var tarea = await _context.Tareas.FindAsync(id);

        if (tarea == null)
            return null;

        tarea.Titulo = dto.Titulo;
        tarea.Estado = dto.Estado;
        tarea.Descripcion = dto.Descripcion;
        tarea.FechaInicio = dto.FechaInicio;
        tarea.FechaFin = dto.FechaFin;
        tarea.FechaModificacion = DateTime.Now;

        await _context.SaveChangesAsync();

        return new TareaDto
        {
            Id = tarea.Id,
            Titulo = tarea.Titulo,
            Estado = tarea.Estado,
            ProyectoId = tarea.ProyectoId,
            Descripcion = tarea.Descripcion,
            FechaInicio = tarea.FechaInicio,
            FechaFin = tarea.FechaFin
        };
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var tarea = await _context.Tareas.FindAsync(id);

        if (tarea == null)
            return false;

        _context.Tareas.Remove(tarea);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> MarcarComoTerminadaAsync(int id)
    {
        var tarea = await _context.Tareas
            .Include(t => t.Proyecto)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tarea == null)
            return false;

        tarea.Estado = "Terminada";
        tarea.FechaFin = DateTime.Now;
        tarea.FechaModificacion = DateTime.Now;

        await _context.SaveChangesAsync();

        await _notificacionService.CrearNotificacionAsync(new CrearNotificacionDto
        {
            Titulo = "Tarea terminada",
            Mensaje = $"La tarea '{tarea.Titulo}' del proyecto '{tarea.Proyecto.Nombre}' fue marcada como terminada.",
            Tipo = "Exito",
            UsuarioId = null
        });

        return true;
    }

    public async Task<bool> AsignarEmpleadoAsync(AsignarEmpleadoTareaDto dto)
    {
        var tarea = await _context.Tareas
            .Include(t => t.Proyecto)
            .FirstOrDefaultAsync(t => t.Id == dto.TareaId);

        var usuario = await _context.Usuario
            .FirstOrDefaultAsync(u =>
                u.Id == dto.UsuarioId &&
                u.Estado != "Baja");

        if (tarea == null || usuario == null)
            return false;

        var yaExiste = await _context.TareaAsignaciones
            .AnyAsync(x => x.TareaId == dto.TareaId &&
                        x.UsuarioId == dto.UsuarioId &&
                        x.Activo);

        if (yaExiste)
            return false;

        var asignacion = new TareaAsignacion
        {
            TareaId = dto.TareaId,
            UsuarioId = dto.UsuarioId,
            FechaAsignacion = DateTime.Now,
            Activo = true
        };

        _context.TareaAsignaciones.Add(asignacion);
        await _context.SaveChangesAsync();

        await _notificacionService.CrearNotificacionAsync(new CrearNotificacionDto
        {
            Titulo = "Asignación a tarea",
            Mensaje = $"Has sido asignado a la tarea '{tarea.Titulo}' del proyecto '{tarea.Proyecto.Nombre}'.",
            Tipo = "Info",
            UsuarioId = dto.UsuarioId
        });

        return true;
    }

    public async Task<bool> EditarAsignacionEmpleadoAsync(int asignacionId, EditarAsignacionTareaDto dto)
    {
        var asignacion = await _context.TareaAsignaciones.FindAsync(asignacionId);

        if (asignacion == null)
            return false;

        var usuarioExiste = await _context.Usuario.AnyAsync(u => u.Id == dto.UsuarioId);

        if (!usuarioExiste)
            return false;

        asignacion.UsuarioId = dto.UsuarioId;
        asignacion.Activo = dto.Activo;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> EliminarAsignacionEmpleadoAsync(int asignacionId)
    {
        var asignacion = await _context.TareaAsignaciones.FindAsync(asignacionId);

        if (asignacion == null)
            return false;

        _context.TareaAsignaciones.Remove(asignacion);
        await _context.SaveChangesAsync();

        return true;
    }
}
