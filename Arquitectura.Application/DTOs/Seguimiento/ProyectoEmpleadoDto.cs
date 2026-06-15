using System;
namespace Arquitectura.Application.DTOs.Seguimiento;

public class ProyectoEmpleadoDto
{
    public int Id { get; set; }

    public int ProyectoId { get; set; }

    public int UsuarioId { get; set; }

    public string NombreEmpleado { get; set; } = string.Empty;

    public string RolProyecto { get; set; } = string.Empty;

    public DateTime FechaAsignacion { get; set; }

    public bool Activo { get; set; }
}
