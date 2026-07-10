using Arquitectura.Application.DTOs.Cuenta;

namespace Arquitectura.Application.Interfaces.Cuenta;

public interface ICuentaService
{
    Task<bool> SolicitarBajaCuentaAsync(
        int usuarioId,
        SolicitarBajaCuentaDto dto);
}