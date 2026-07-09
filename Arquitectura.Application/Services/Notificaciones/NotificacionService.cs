using Arquitectura.Application.DTOs.Notificaciones;
using Arquitectura.Application.Interfaces.Notificaciones;
using Arquitectura.Domain.Entities;
using Arquitectura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Arquitectura.Application.Services.Notificaciones;

public class NotificacionService : INotificacionService
{
    private readonly ArquitecturaDbContext _context;

    public NotificacionService(ArquitecturaDbContext context)
    {
        _context = context;
    }

    public async Task<int> CrearNotificacionAsync(CrearNotificacionDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Titulo))
            throw new Exception("El título de la notificación es obligatorio.");

        if (string.IsNullOrWhiteSpace(dto.Mensaje))
            throw new Exception("El mensaje de la notificación es obligatorio.");

        if (dto.UsuarioId.HasValue)
        {
            var usuarioExiste = await _context.Usuario
                .AnyAsync(u => u.Id == dto.UsuarioId.Value);

            if (!usuarioExiste)
                throw new Exception("El usuario indicado no existe.");
        }

        var notificacion = new Notificacion
        {
            Titulo = dto.Titulo,
            Mensaje = dto.Mensaje,
            Tipo = string.IsNullOrWhiteSpace(dto.Tipo) ? "Info" : dto.Tipo,
            UsuarioId = dto.UsuarioId,
            Leida = false,
            FechaCreacion = DateTime.Now
        };

        _context.Notificaciones.Add(notificacion);
        await _context.SaveChangesAsync();

        return notificacion.Id;
    }

    public async Task<List<NotificacionDto>> ObtenerNotificacionesAsync()
    {
        return await _context.Notificaciones
            .Include(n => n.Usuario)
            .OrderByDescending(n => n.FechaCreacion)
            .Select(n => new NotificacionDto
            {
                Id = n.Id,
                Titulo = n.Titulo,
                Mensaje = n.Mensaje,
                Tipo = n.Tipo,
                UsuarioId = n.UsuarioId,
                NombreUsuario = n.Usuario == null
                    ? null
                    : n.Usuario.Nombre + " " + n.Usuario.Apellidos,
                Leida = n.Leida,
                FechaCreacion = n.FechaCreacion,
                FechaLectura = n.FechaLectura
            })
            .ToListAsync();
    }

    public async Task<List<NotificacionDto>> ObtenerNotificacionesPorUsuarioAsync(int usuarioId)
    {
        return await _context.Notificaciones
            .Include(n => n.Usuario)
            .Where(n => n.UsuarioId == usuarioId || n.UsuarioId == null)
            .OrderByDescending(n => n.FechaCreacion)
            .Select(n => new NotificacionDto
            {
                Id = n.Id,
                Titulo = n.Titulo,
                Mensaje = n.Mensaje,
                Tipo = n.Tipo,
                UsuarioId = n.UsuarioId,
                NombreUsuario = n.Usuario == null
                    ? null
                    : n.Usuario.Nombre + " " + n.Usuario.Apellidos,
                Leida = n.Leida,
                FechaCreacion = n.FechaCreacion,
                FechaLectura = n.FechaLectura
            })
            .ToListAsync();
    }

    public async Task<List<NotificacionDto>> ObtenerNoLeidasAsync()
    {
        return await _context.Notificaciones
            .Include(n => n.Usuario)
            .Where(n => !n.Leida)
            .OrderByDescending(n => n.FechaCreacion)
            .Select(n => new NotificacionDto
            {
                Id = n.Id,
                Titulo = n.Titulo,
                Mensaje = n.Mensaje,
                Tipo = n.Tipo,
                UsuarioId = n.UsuarioId,
                NombreUsuario = n.Usuario == null
                    ? null
                    : n.Usuario.Nombre + " " + n.Usuario.Apellidos,
                Leida = n.Leida,
                FechaCreacion = n.FechaCreacion,
                FechaLectura = n.FechaLectura
            })
            .ToListAsync();
    }

    public async Task<bool> MarcarComoLeidaAsync(int id)
    {
        var notificacion = await _context.Notificaciones
            .FirstOrDefaultAsync(n => n.Id == id);

        if (notificacion == null)
            return false;

        notificacion.Leida = true;
        notificacion.FechaLectura = DateTime.Now;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> EliminarNotificacionAsync(int id)
    {
        var notificacion = await _context.Notificaciones
            .FirstOrDefaultAsync(n => n.Id == id);

        if (notificacion == null)
            return false;

        _context.Notificaciones.Remove(notificacion);
        await _context.SaveChangesAsync();

        return true;
    }
}