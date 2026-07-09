namespace Arquitectura.Application.DTOs.Notificaciones;

public class CrearNotificacionDto
{
    public string Titulo { get; set; } = string.Empty;

    public string Mensaje { get; set; } = string.Empty;

    public string Tipo { get; set; } = "Info";

    public int? UsuarioId { get; set; }
}