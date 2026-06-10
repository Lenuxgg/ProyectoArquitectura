using Arquitectura.Application.DTOs.Seguimiento;

namespace Arquitectura.Application.Interfaces.Seguimiento;

public interface ITareaService
{
    Task<List<TareaDto>> ObtenerTodasAsync();

    Task<List<TareaDto>> ObtenerPorProyectoAsync(int proyectoId);

    Task<TareaDto?> ObtenerPorIdAsync(int id);

    Task<TareaDto> CrearAsync(CrearTareaDto dto);

    Task<TareaDto?> EditarAsync(int id, EditarTareaDto dto);

    Task<bool> EliminarAsync(int id);

    Task<bool> MarcarComoTerminadaAsync(int id);

    Task<bool> AsignarEmpleadoAsync(AsignarEmpleadoTareaDto dto);

    Task<bool> EditarAsignacionEmpleadoAsync(int asignacionId, EditarAsignacionTareaDto dto);

    Task<bool> EliminarAsignacionEmpleadoAsync(int asignacionId);
}