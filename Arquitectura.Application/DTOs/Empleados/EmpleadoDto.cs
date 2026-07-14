using System.ComponentModel.DataAnnotations;

namespace Arquitectura.Application.DTOs.Empleados;

public class EmpleadoDto
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
}

public class CrearEmpleadoDto
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

    [Range(typeof(decimal), "0", "999999999999.99", ErrorMessage = "El salario no puede ser negativo.")]
    public decimal? Salario { get; set; }

    public DateTime? FechaContratacion { get; set; }

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres.")]
    public string Password { get; set; } = string.Empty;
}

public class ActualizarPuestoDto
{
    [Required(ErrorMessage = "El puesto es obligatorio.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El puesto debe tener entre 2 y 100 caracteres.")]
    public string Puesto { get; set; } = string.Empty;
}

public class ActualizarSalarioDto
{
    [Range(typeof(decimal), "0.01", "999999999999.99", ErrorMessage = "El salario debe ser mayor a 0.")]
    public decimal Salario { get; set; }
}