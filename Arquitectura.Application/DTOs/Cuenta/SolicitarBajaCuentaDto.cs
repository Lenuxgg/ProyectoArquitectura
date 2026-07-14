using System.ComponentModel.DataAnnotations;

namespace Arquitectura.Application.DTOs.Cuenta;

public class SolicitarBajaCuentaDto
{
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres.")]
    public string Password { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "El motivo no puede superar los 300 caracteres.")]
    public string? Motivo { get; set; }
}