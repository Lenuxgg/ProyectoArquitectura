using Arquitectura.Application.DTOs.Notificaciones;

namespace Arquitectura.Application.Interfaces.Notificaciones;

public interface INotificacionService
{
    Task<int> CrearNotificacionAsync(CrearNotificacionDto dto);

    Task<List<NotificacionDto>> ObtenerNotificacionesAsync();

    Task<List<NotificacionDto>> ObtenerNotificacionesPorUsuarioAsync(int usuarioId);

    Task<List<NotificacionDto>> ObtenerNoLeidasAsync();

    Task<bool> MarcarComoLeidaAsync(int id);

    Task<bool> EliminarNotificacionAsync(int id);
}