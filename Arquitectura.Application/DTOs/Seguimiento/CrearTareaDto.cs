namespace Arquitectura.Application.DTOs.Seguimiento;

public class CrearTareaDto
{
    public string Titulo { get; set; } = string.Empty;

    public int ProyectoId { get; set; }

    public string? Descripcion { get; set; }

    public DateTime? FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }
}