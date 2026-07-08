namespace Arquitectura.Application.DTOs.Contabilidad;

public class NominaResultadoDto
{
    public int Id { get; set; }

    public DateTime PeriodoInicio { get; set; }

    public DateTime PeriodoFin { get; set; }

    public string Estado { get; set; } = string.Empty;

    public decimal TotalBruto { get; set; }

    public decimal TotalDeducciones { get; set; }

    public decimal TotalNeto { get; set; }

    public int UsuarioId { get; set; }

    public DateTime FechaRegistro { get; set; }

    public DateTime? FechaAprobacion { get; set; }

    public List<NominaDetalleDto> Detalles { get; set; } = new();
}