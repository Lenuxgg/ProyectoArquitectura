namespace Arquitectura.Application.DTOs.Seguimiento;

public class DocumentoProyectoDto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string RutaArchivo { get; set; } = string.Empty;

    public int ProyectoId { get; set; }

    public DateTime FechaCarga { get; set; }
}
