namespace Arquitectura.Application.DTOs.Contabilidad;

public class NominaDetalleDto
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public string NombreEmpleado { get; set; } = string.Empty;

    public decimal SalarioBase { get; set; }

    public decimal Deducciones { get; set; }

    public decimal Bonificaciones { get; set; }

    public decimal SalarioNeto { get; set; }

    public bool Inconsistencia { get; set; }

    public string? DetalleInconsistencia { get; set; }
}