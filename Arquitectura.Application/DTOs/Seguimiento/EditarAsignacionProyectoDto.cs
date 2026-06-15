namespace Arquitectura.Application.DTOs.Seguimiento;

public class EditarAsignacionProyectoDto
{
    public int UsuarioId { get; set; }

    public string RolProyecto { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;
}
