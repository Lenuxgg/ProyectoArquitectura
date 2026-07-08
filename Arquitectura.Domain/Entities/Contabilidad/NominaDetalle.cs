namespace Arquitectura.Domain.Entities;

public class NominaDetalle
{
    public int Id { get; set; }

    public int NominaId { get; set; }

    public int UsuarioId { get; set; }

    public decimal SalarioBase { get; set; }

    public decimal Deducciones { get; set; }

    public decimal Bonificaciones { get; set; }

    public decimal SalarioNeto { get; set; }

    public bool Inconsistencia { get; set; }

    public string? DetalleInconsistencia { get; set; }

    public Nomina Nomina { get; set; } = null!;

    public Usuario Usuario { get; set; } = null!;
}