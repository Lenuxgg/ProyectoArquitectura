using System.ComponentModel.DataAnnotations;

namespace Arquitectura.Application.DTOs.Contabilidad;

public class ProcesarNominaDto
{
    [Range(2020, 2100, ErrorMessage = "El año debe estar entre 2020 y 2100.")]
    public int Anio { get; set; }

    [Range(1, 12, ErrorMessage = "El mes debe estar entre 1 y 12.")]
    public int Mes { get; set; }

    [Range(typeof(decimal), "0", "100", ErrorMessage = "El porcentaje de deducción debe estar entre 0 y 100.")]
    public decimal PorcentajeDeduccion { get; set; } = 10;

    [Range(typeof(decimal), "0", "999999999999.99", ErrorMessage = "La bonificación no puede ser negativa.")]
    public decimal BonificacionGeneral { get; set; } = 0;
}