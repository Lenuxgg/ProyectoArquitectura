namespace Arquitectura.Application.DTOs.Contabilidad;

public class ValidacionNominaDto
{
    public DateTime PeriodoInicio { get; set; }

    public DateTime PeriodoFin { get; set; }

    public bool TieneInconsistencias { get; set; }

    public int TotalInconsistencias { get; set; }

    public List<InconsistenciaNominaDto> Inconsistencias { get; set; } = new();
}

public class InconsistenciaNominaDto
{
    public string Tipo { get; set; } = string.Empty;

    public int? UsuarioId { get; set; }

    public string? NombreEmpleado { get; set; }

    public string Detalle { get; set; } = string.Empty;
}