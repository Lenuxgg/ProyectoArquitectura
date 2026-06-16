using Arquitectura.Application.DTOs.Administracion;
using Arquitectura.Application.DTOs.Auth;

namespace Arquitectura.Application.Interfaces.Administracion;

public interface IUsuarioService
{
    Task<List<UsuarioDto>> GetAllAsync();
    Task<UsuarioDto?> GetByIdAsync(int id);
    Task<List<UsuarioDto>> BuscarAsync(string termino);
    Task<UsuarioDto> CreateAsync(CrearUsuarioDto dto);
    Task<UsuarioDto?> UpdateAsync(int id, ActualizarUsuarioDto dto);
    Task<bool> DeleteAsync(int id);
    Task<bool> AsignarRolAsync(AsignarRolDto dto);
    Task<bool> RemoverRolAsync(int usuarioId, int rolId);
    Task<UsuarioDto?> LoginAsync(LoginDto dto);
}