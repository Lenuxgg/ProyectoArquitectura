namespace Arquitectura.Application.DTOs.Administracion;

public class RolDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public class CrearRolDto
{
    public string Nombre { get; set; } = string.Empty;
}

public class ActualizarRolDto
{
    public string Nombre { get; set; } = string.Empty;
}