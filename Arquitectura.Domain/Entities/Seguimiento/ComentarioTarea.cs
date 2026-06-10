namespace Arquitectura.Domain.Entities;

public class ComentarioTarea
{
    public int Id { get; set; }

    public string Texto { get; set; } = string.Empty;

    public string? ArchivoRuta { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Now;

    public int TareaId { get; set; }

    public int UsuarioId { get; set; }

    public Tarea Tarea { get; set; } = null!;

    public Usuario Usuario { get; set; } = null!;
}
