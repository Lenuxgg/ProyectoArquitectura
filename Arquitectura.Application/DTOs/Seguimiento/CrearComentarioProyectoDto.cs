namespace Arquitectura.Application.DTOs.Seguimiento;

public class CrearComentarioProyectoDto
{
    public int ProyectoId { get; set; }

    public string Text { get; set; } = string.Empty;

    public string? ArchivoRuta { get; set; }
}