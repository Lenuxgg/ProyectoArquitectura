using System.ComponentModel.DataAnnotations;

namespace Arquitectura.Application.DTOs.Administracion;

public class RolDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public class CrearRolDto
{
    [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre del rol debe tener entre 3 y 50 caracteres.")]
    public string Nombre { get; set; } = string.Empty;
}

public class ActualizarRolDto
{
    [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre del rol debe tener entre 3 y 50 caracteres.")]
    public string Nombre { get; set; } = string.Empty;
}