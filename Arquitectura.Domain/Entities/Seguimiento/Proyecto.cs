using System.Threading;

namespace Arquitectura.Domain.Entities;

public class Proyecto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public DateTime FechaInicio { get; set; } = DateTime.Now;

    public DateTime? FechaFin { get; set; }

    public string Estado { get; set; } = "Activo";

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public DateTime? FechaModificacion { get; set; }

    public ICollection<Tarea> Tareas { get; set; } = new List<Tarea>();

    public ICollection<ComentarioProyecto> Comentarios { get; set; } = new List<ComentarioProyecto>();

    public ICollection<ProyectoEmpleado> ProyectoEmpleados { get; set; } = new List<ProyectoEmpleado>();

    public ICollection<DocumentoProyecto> Documentos { get; set; } = new List<DocumentoProyecto>();
}