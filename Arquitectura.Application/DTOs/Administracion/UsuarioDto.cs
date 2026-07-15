using System.ComponentModel.DataAnnotations;

namespace Arquitectura.Application.DTOs.Administracion;

public class UsuarioDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Puesto { get; set; }
    public decimal? Salario { get; set; }
    public DateTime? FechaContratacion { get; set; }
    public string Estado { get; set; } = string.Empty;
    public bool Admin { get; set; }
    public List<string> Roles { get; set; } = new();
}

public class CrearUsuarioDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Los apellidos deben tener entre 2 y 100 caracteres.")]
    public string Apellidos { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido.")]
    [StringLength(150, ErrorMessage = "El email no puede superar los 150 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "El teléfono no puede superar los 20 caracteres.")]
    public string? Telefono { get; set; }

    [StringLength(100, ErrorMessage = "El puesto no puede superar los 100 caracteres.")]
    public string? Puesto { get; set; }

    [Range(0.0, 999999999999.99, ErrorMessage = "El salario no puede ser negativo.")]
    public decimal? Salario { get; set; }

    public DateTime? FechaContratacion { get; set; }

    public bool Admin { get; set; } = false;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres.")]
    public string Password { get; set; } = string.Empty;

    public List<int> RolesIds { get; set; } = new();
}

public class ActualizarUsuarioDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Los apellidos deben tener entre 2 y 100 caracteres.")]
    public string Apellidos { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "El teléfono no puede superar los 20 caracteres.")]
    public string? Telefono { get; set; }

    [StringLength(100, ErrorMessage = "El puesto no puede superar los 100 caracteres.")]
    public string? Puesto { get; set; }

    [Range(0.0, 999999999999.99, ErrorMessage = "El salario no puede ser negativo.")]
    public decimal? Salario { get; set; }

    public DateTime? FechaContratacion { get; set; }

    [Required(ErrorMessage = "El estado es obligatorio.")]
    [RegularExpression("^(Activo|Inactivo|Baja)$", ErrorMessage = "El estado debe ser Activo, Inactivo o Baja.")]
    public string Estado { get; set; } = string.Empty;

    public bool Admin { get; set; }
}

public class AsignarRolDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El usuario es obligatorio.")]
    public int UsuarioId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El rol es obligatorio.")]
    public int RolId { get; set; }
}