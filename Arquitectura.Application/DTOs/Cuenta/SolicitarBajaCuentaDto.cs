namespace Arquitectura.Application.DTOs.Cuenta;

public class SolicitarBajaCuentaDto
{
    public string Password { get; set; } = string.Empty;

    public string? Motivo { get; set; }
}