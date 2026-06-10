namespace Arquitectura.Domain.Entities;

public class ComentarioProyecto
{
    public int Id { get; set; }

    public int ProyectoId { get; set; }

    public string? ArchivoRuta { get; set; }

    public string Text { get; set; } = string.Empty;

    public DateTime Fecha { get; set; } = DateTime.Now;

    public Proyecto Proyecto { get; set; } = null!;
}
