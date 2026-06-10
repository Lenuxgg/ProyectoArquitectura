namespace Arquitectura.Domain.Entities;

public class ProyectoEmpleado
{
    public int Id { get; set; }

    public int ProyectoId { get; set; }

    public int UsuarioId { get; set; }

    public DateTime FechaAsignacion { get; set; } = DateTime.Now;

    public bool Activo { get; set; } = true;

    public Proyecto Proyecto { get; set; } = null!;

    public Usuario Usuario { get; set; } = null!;
}