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
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Puesto { get; set; }
    public decimal? Salario { get; set; }
    public DateTime? FechaContratacion { get; set; }
    public string Password { get; set; } = string.Empty;
}

public class ActualizarPuestoDto
{
    public string Puesto { get; set; } = string.Empty;
}

public class ActualizarSalarioDto
{
    public decimal Salario { get; set; }
}