using System.Security.Cryptography;
using System.Text;
using Arquitectura.Application.DTOs.Cuenta;
using Arquitectura.Application.DTOs.Notificaciones;
using Arquitectura.Application.Interfaces.Cuenta;
using Arquitectura.Application.Interfaces.Notificaciones;
using Arquitectura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Arquitectura.Application.Services.Cuenta;

public class CuentaService : ICuentaService
{
    private readonly ArquitecturaDbContext _context;
    private readonly INotificacionService _notificacionService;

    public CuentaService(
        ArquitecturaDbContext context,
        INotificacionService notificacionService)
    {
        _context = context;
        _notificacionService = notificacionService;
    }

    public async Task<bool> SolicitarBajaCuentaAsync(
        int usuarioId,
        SolicitarBajaCuentaDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Password))
            throw new Exception("Debe ingresar la contraseña para solicitar la baja de la cuenta.");

        var usuario = await _context.Usuario
            .FirstOrDefaultAsync(u => u.Id == usuarioId && u.Estado != "Baja");

        if (usuario == null)
            return false;

        var passwordHash = HashPassword(dto.Password);

        if (usuario.PasswordHash != passwordHash)
            throw new Exception("La contraseña ingresada no es correcta.");

        usuario.Estado = "Baja";
        usuario.FechaModificacion = DateTime.Now;

        await _context.SaveChangesAsync();

        var nombreUsuario = $"{usuario.Nombre} {usuario.Apellidos}".Trim();

        await _notificacionService.CrearNotificacionAsync(new CrearNotificacionDto
        {
            Titulo = "Solicitud de baja de cuenta",
            Mensaje = string.IsNullOrWhiteSpace(dto.Motivo)
                ? $"El usuario {nombreUsuario} solicitó la baja de su cuenta."
                : $"El usuario {nombreUsuario} solicitó la baja de su cuenta. Motivo: {dto.Motivo}",
            Tipo = "Advertencia",
            UsuarioId = null
        });

        return true;
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }
}