using Arquitectura.Application.DTOs.Contabilidad;

namespace Arquitectura.Application.Interfaces.Contabilidad;

public interface IContabilidadService
{
    Task<int> RegistrarIngresoAsync(
        RegistrarTransaccionDto dto,
        int usuarioId);

    Task<int> RegistrarEgresoAsync(
        RegistrarTransaccionDto dto,
        int usuarioId);

        Task<List<TransaccionDto>> ObtenerIngresosAsync();

        Task<List<TransaccionDto>> ObtenerEgresosAsync();

        Task<bool> EliminarTransaccionAsync(int id);
}