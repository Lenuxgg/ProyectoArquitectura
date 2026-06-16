using Arquitectura.Application.DTOs.Seguimiento;

namespace Arquitectura.Application.Interfaces.Seguimiento;

public interface IProyectoService
{
    Task<List<ProyectoDto>> ObtenerTodosAsync();

    Task<ProyectoDto?> ObtenerPorIdAsync(int id);

    Task<ProyectoDto> CrearAsync(CrearProyectoDto dto);

    Task<ProyectoDto?> EditarAsync(int id, EditarProyectoDto dto);

    Task<bool> TerminarProyectoAsync(int id);

    Task<bool> EliminarAsync(int id);

    Task<bool> AsignarEmpleadoAsync(AsignarEmpleadoProyectoDto dto);

    Task<bool> EditarAsignacionEmpleadoAsync(int asignacionId, EditarAsignacionProyectoDto dto);

    Task<bool> EliminarEmpleadoProyectoAsync(int asignacionId);

    Task<List<ProyectoEmpleadoDto>> ObtenerEmpleadosPorProyectoAsync(int proyectoId);

    Task<DocumentoProyectoDto?> AdjuntarDocumentoProyectoAsync(int proyectoId, string nombre, string rutaArchivo);

    Task<List<DocumentoProyectoDto>> ObtenerDocumentosPorProyectoAsync(int proyectoId);

    Task<bool> EliminarDocumentoProyectoAsync(int documentoId);
}
