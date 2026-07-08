namespace Arquitectura.Domain.Entities;

public class Nomina
{
    public int Id { get; set; }

    public DateTime PeriodoInicio { get; set; }

    public DateTime PeriodoFin { get; set; }

    public string Estado { get; set; } = "Pendiente";

    public decimal TotalBruto { get; set; }

    public decimal TotalDeducciones { get; set; }

    public decimal TotalNeto { get; set; }

    public int UsuarioId { get; set; }

    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    public DateTime? FechaAprobacion { get; set; }

    public Usuario Usuario { get; set; } = null!;

    public ICollection<NominaDetalle> Detalles { get; set; } = new List<NominaDetalle>();
}