namespace Arquitectura.Application.DTOs.Seguimiento;

public class ComentarioProyectoDto
{
    public int Id { get; set; }

    public int ProyectoId { get; set; }

    public string Text { get; set; } = string.Empty;

    public string? ArchivoRuta { get; set; }

    public DateTime Fecha { get; set; }
}