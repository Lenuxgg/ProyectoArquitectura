namespace Arquitectura.Application.DTOs.Seguimiento;

public class EditarProyectoDto
{
    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    public string Estado { get; set; } = "Activo";
}