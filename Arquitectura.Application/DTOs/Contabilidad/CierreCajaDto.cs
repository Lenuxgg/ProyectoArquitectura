namespace Arquitectura.Application.DTOs.Contabilidad;

public class CierreCajaDto
{
    public string TipoCierre { get; set; } = string.Empty;

    public DateTime FechaInicio { get; set; }

    public DateTime FechaFin { get; set; }

    public decimal TotalIngresos { get; set; }

    public decimal TotalEgresos { get; set; }

    public decimal Balance { get; set; }

    public int CantidadIngresos { get; set; }

    public int CantidadEgresos { get; set; }
}