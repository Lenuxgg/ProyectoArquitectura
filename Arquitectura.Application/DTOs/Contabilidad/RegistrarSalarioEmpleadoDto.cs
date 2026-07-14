using System.ComponentModel.DataAnnotations;

namespace Arquitectura.Application.DTOs.Contabilidad;

public class RegistrarSalarioEmpleadoDto
{
    [Range(typeof(decimal), "0.01", "999999999999.99", ErrorMessage = "El salario debe ser mayor a 0.")]
    public decimal Salario { get; set; }
}