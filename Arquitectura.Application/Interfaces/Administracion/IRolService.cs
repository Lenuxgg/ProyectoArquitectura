using Arquitectura.Application.DTOs.Administracion;

namespace Arquitectura.Application.Interfaces.Administracion;

public interface IRolService
{
    Task<List<RolDto>> GetAllAsync();
    Task<RolDto?> GetByIdAsync(int id);
    Task<RolDto> CreateAsync(CrearRolDto dto);
    Task<RolDto?> UpdateAsync(int id, ActualizarRolDto dto);
    Task<bool> DeleteAsync(int id);
}