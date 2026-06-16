using Arquitectura.Application.DTOs.Seguimiento;
using Arquitectura.Application.Interfaces.Seguimiento;
using Arquitectura.Domain.Entities;
using Arquitectura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Arquitectura.Application.Services.Seguimiento;

public class ComentarioProyectoService : IComentarioProyectoService
{
    private readonly ArquitecturaDbContext _context;

    public ComentarioProyectoService(ArquitecturaDbContext context)
    {
        _context = context;
    }

    public async Task<List<ComentarioProyectoDto>> ObtenerTodosAsync()
    {
        return await _context.ComentarioProyectos
            .Select(c => new ComentarioProyectoDto
            {
                Id = c.Id,
                ProyectoId = c.ProyectoId,
                Text = c.Text,
                ArchivoRuta = c.ArchivoRuta,
                Fecha = c.Fecha
            })
            .ToListAsync();
    }

    public async Task<List<ComentarioProyectoDto>> ObtenerPorProyectoAsync(int proyectoId)
    {
        return await _context.ComentarioProyectos
            .Where(c => c.ProyectoId == proyectoId)
            .Select(c => new ComentarioProyectoDto
            {
                Id = c.Id,
                ProyectoId = c.ProyectoId,
                Text = c.Text,
                ArchivoRuta = c.ArchivoRuta,
                Fecha = c.Fecha
            })
            .ToListAsync();
    }

    public async Task<ComentarioProyectoDto?> ObtenerPorIdAsync(int id)
    {
        return await _context.ComentarioProyectos
            .Where(c => c.Id == id)
            .Select(c => new ComentarioProyectoDto
            {
                Id = c.Id,
                ProyectoId = c.ProyectoId,
                Text = c.Text,
                ArchivoRuta = c.ArchivoRuta,
                Fecha = c.Fecha
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ComentarioProyectoDto> CrearAsync(CrearComentarioProyectoDto dto)
    {
        var comentario = new ComentarioProyecto
        {
            ProyectoId = dto.ProyectoId,
            Text = dto.Text,
            ArchivoRuta = dto.ArchivoRuta,
            Fecha = DateTime.Now
        };

        _context.ComentarioProyectos.Add(comentario);
        await _context.SaveChangesAsync();

        return new ComentarioProyectoDto
        {
            Id = comentario.Id,
            ProyectoId = comentario.ProyectoId,
            Text = comentario.Text,
            ArchivoRuta = comentario.ArchivoRuta,
            Fecha = comentario.Fecha
        };
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var comentario = await _context.ComentarioProyectos.FindAsync(id);

        if (comentario == null)
            return false;

        _context.ComentarioProyectos.Remove(comentario);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<ComentarioProyectoDto?> AdjuntarArchivoAsync(int comentarioId, string archivoRuta)
    {
        var comentario = await _context.ComentarioProyectos.FindAsync(comentarioId);

        if (comentario == null)
            return null;

        comentario.ArchivoRuta = archivoRuta;

        await _context.SaveChangesAsync();

        return new ComentarioProyectoDto
        {
            Id = comentario.Id,
            ProyectoId = comentario.ProyectoId,
            Text = comentario.Text,
            ArchivoRuta = comentario.ArchivoRuta,
            Fecha = comentario.Fecha
        };
    }
}