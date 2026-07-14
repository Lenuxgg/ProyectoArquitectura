namespace Arquitectura.Application.DTOs.Contabilidad;

public class ReporteProyectoFinancieroDto
{
    public int ProyectoId { get; set; }

    public string ProyectoNombre { get; set; } = string.Empty;

    public decimal TotalIngresos { get; set; }

    public decimal TotalEgresos { get; set; }

    public decimal Balance { get; set; }

    public int CantidadIngresos { get; set; }

    public int CantidadEgresos { get; set; }

    public List<TransaccionDto> Transacciones { get; set; } = new();
}