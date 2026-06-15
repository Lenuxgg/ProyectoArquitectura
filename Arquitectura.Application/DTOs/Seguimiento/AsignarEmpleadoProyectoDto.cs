namespace Arquitectura.Application.DTOs.Seguimiento;

public class AsignarEmpleadoProyectoDto
{
    public int ProyectoId { get; set; }

    public int UsuarioId { get; set; }

    public string RolProyecto { get; set; } = string.Empty;
}

