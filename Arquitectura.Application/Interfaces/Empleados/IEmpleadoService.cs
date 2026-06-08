using Arquitectura.Application.DTOs.Empleados;

namespace Arquitectura.Application.Interfaces.Empleados;

public interface IEmpleadoService
{
    Task<List<EmpleadoDto>> GetAllAsync();
    Task<EmpleadoDto?> GetByIdAsync(int id);
    Task<EmpleadoDto> CrearEmpleadoAsync(CrearEmpleadoDto dto);
    Task<EmpleadoDto?> ActualizarPuestoAsync(int id, ActualizarPuestoDto dto);
    Task<EmpleadoDto?> ActualizarSalarioAsync(int id, ActualizarSalarioDto dto);
    Task<bool> DarDeBajaAsync(int id);
}