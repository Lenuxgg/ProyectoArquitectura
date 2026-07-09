namespace Arquitectura.Application.DTOs.Exportaciones;

public class ExportacionArchivoDto
{
    public string NombreArchivo { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public byte[] Contenido { get; set; } = Array.Empty<byte>();
}