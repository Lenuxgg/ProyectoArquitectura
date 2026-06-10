namespace Arquitectura.Domain.Entities;

public class TareaAsignacion
{
    public int Id { get; set; }

    public int TareaId { get; set; }

    public int UsuarioId { get; set; }

    public DateTime FechaAsignacion { get; set; } = DateTime.Now;

    public bool Activo { get; set; } = true;

    public Tarea Tarea { get; set; } = null!;

    public Usuario Usuario { get; set; } = null!;
}
