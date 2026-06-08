namespace Arquitectura.Application.DTOs.Administracion;

public class UsuarioDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Puesto { get; set; }
    public decimal? Salario { get; set; }
    public DateTime? FechaContratacion { get; set; }
    public string Estado { get; set; } = string.Empty;
    public bool Admin { get; set; }
    public List<string> Roles { get; set; } = new();
}

public class CrearUsuarioDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Puesto { get; set; }
    public decimal? Salario { get; set; }
    public DateTime? FechaContratacion { get; set; }
    public bool Admin { get; set; } = false;
    public string Password { get; set; } = string.Empty;
    public List<int> RolesIds { get; set; } = new();
}

public class ActualizarUsuarioDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Puesto { get; set; }
    public decimal? Salario { get; set; }
    public DateTime? FechaContratacion { get; set; }
    public string Estado { get; set; } = string.Empty;
    public bool Admin { get; set; }
}

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AsignarRolDto
{
    public int UsuarioId { get; set; }
    public int RolId { get; set; }
}