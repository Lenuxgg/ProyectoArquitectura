namespace Arquitectura.Application.DTOs.Seguimiento;

public class EditarTareaDto
{
    public string Titulo { get; set; } = string.Empty;

    public string Estado { get; set; } = "Pendiente";

    public string? Descripcion { get; set; }

    public DateTime? FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }
}