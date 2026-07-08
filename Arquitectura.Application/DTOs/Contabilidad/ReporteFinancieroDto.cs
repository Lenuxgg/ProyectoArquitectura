namespace Arquitectura.Application.DTOs.Contabilidad;

public class ReporteFinancieroDto
{
    public decimal TotalIngresos { get; set; }

    public decimal TotalEgresos { get; set; }

    public decimal Balance { get; set; }

    public int CantidadIngresos { get; set; }

    public int CantidadEgresos { get; set; }
}