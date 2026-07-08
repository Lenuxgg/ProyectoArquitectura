namespace Arquitectura.Application.DTOs.Contabilidad;

public class ProcesarNominaDto
{
    public int Anio { get; set; }

    public int Mes { get; set; }

    public decimal PorcentajeDeduccion { get; set; } = 10;

    public decimal BonificacionGeneral { get; set; } = 0;
}