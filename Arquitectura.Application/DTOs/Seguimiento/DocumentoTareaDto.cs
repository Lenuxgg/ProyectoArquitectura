namespace Arquitectura.Application.DTOs.Seguimiento;

public class DocumentoTareaDto
{
    public int Id { get; set; }

    public int TareaId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string RutaArchivo { get; set; } = string.Empty;

    public DateTime Fecha { get; set; }
}
