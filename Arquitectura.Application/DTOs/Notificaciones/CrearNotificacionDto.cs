using System.ComponentModel.DataAnnotations;

namespace Arquitectura.Application.DTOs.Notificaciones;

public class CrearNotificacionDto
{
    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El título debe tener entre 3 y 100 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El mensaje es obligatorio.")]
    [StringLength(500, MinimumLength = 3, ErrorMessage = "El mensaje debe tener entre 3 y 500 caracteres.")]
    public string Mensaje { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo de notificación es obligatorio.")]
    [StringLength(30, ErrorMessage = "El tipo no puede superar los 30 caracteres.")]
    public string Tipo { get; set; } = "Info";

    [Range(1, int.MaxValue, ErrorMessage = "El usuario debe ser válido.")]
    public int? UsuarioId { get; set; }
}