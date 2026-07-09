namespace Arquitectura.Application.DTOs.Notificaciones;

public class NotificacionDto
{
    public int Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Mensaje { get; set; } = string.Empty;

    public string Tipo { get; set; } = string.Empty;

    public int? UsuarioId { get; set; }

    public string? NombreUsuario { get; set; }

    public bool Leida { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaLectura { get; set; }
}