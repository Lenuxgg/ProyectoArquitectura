using Arquitectura.Application.DTOs.Seguimiento;

namespace Arquitectura.Application.Interfaces.Seguimiento;

public interface IComentarioProyectoService
{
    Task<List<ComentarioProyectoDto>> ObtenerTodosAsync();

    Task<List<ComentarioProyectoDto>> ObtenerPorProyectoAsync(int proyectoId);

    Task<ComentarioProyectoDto?> ObtenerPorIdAsync(int id);

    Task<ComentarioProyectoDto> CrearAsync(CrearComentarioProyectoDto dto);

    Task<bool> EliminarAsync(int id);

    Task<ComentarioProyectoDto?> AdjuntarArchivoAsync(int comentarioId, string archivoRuta);
}
