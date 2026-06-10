namespace Arquitectura.Domain.Entities;

public class Tarea
{
    public int Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Estado { get; set; } = "Pendiente";

    public int ProyectoId { get; set; }

    public string? Descripcion { get; set; }

    public DateTime? FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public DateTime? FechaModificacion { get; set; }

    public Proyecto Proyecto { get; set; } = null!;

    public ICollection<ComentarioTarea> Comentarios { get; set; } = new List<ComentarioTarea>();

    public ICollection<TareaAsignacion> Asignaciones { get; set; } = new List<TareaAsignacion>();
}
