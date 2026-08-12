namespace Arquitectura.Application.DTOs.Seguimiento;

public class CrearProyectoDto
{
    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public DateTime FechaInicio { get; set; } = DateTime.Now;

    public DateTime? FechaFin { get; set; }
}