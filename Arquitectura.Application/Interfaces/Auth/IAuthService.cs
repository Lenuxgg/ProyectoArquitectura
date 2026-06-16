using Arquitectura.Application.DTOs.Auth;

namespace Arquitectura.Application.Interfaces.Auth;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginDto dto);
}