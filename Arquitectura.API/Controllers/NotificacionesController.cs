using Arquitectura.Application.DTOs.Notificaciones;
using Arquitectura.Application.Interfaces.Notificaciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Arquitectura.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificacionesController : ControllerBase
{
    private readonly INotificacionService _notificacionService;

    public NotificacionesController(INotificacionService notificacionService)
    {
        _notificacionService = notificacionService;
    }

    private bool UsuarioEsAdministrador()
    {
        return User.IsInRole("Administrador");
    }

    private int? ObtenerUsuarioIdAutenticado()
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            return null;

        return usuarioId;
    }

    private IActionResult Prohibido(string mensaje)
    {
        return StatusCode(StatusCodes.Status403Forbidden, new
        {
            mensaje
        });
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost]
    public async Task<IActionResult> CrearNotificacion([FromBody] CrearNotificacionDto dto)
    {
        try
        {
            var id = await _notificacionService.CrearNotificacionAsync(dto);

            return Ok(new
            {
                mensaje = "Notificación creada correctamente.",
                id
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                mensaje = ex.Message
            });
        }
    }

    [Authorize(Roles = "Administrador")]
    [HttpGet]
    public async Task<IActionResult> ObtenerNotificaciones()
    {
        var lista = await _notificacionService.ObtenerNotificacionesAsync();
        return Ok(lista);
    }

    [HttpGet("usuario/{usuarioId:int}")]
    public async Task<IActionResult> ObtenerNotificacionesPorUsuario(int usuarioId)
    {
        if (!UsuarioEsAdministrador())
        {
            var usuarioAutenticadoId = ObtenerUsuarioIdAutenticado();

            if (usuarioAutenticadoId == null)
                return Unauthorized("No se pudo identificar el usuario autenticado.");

            if (usuarioAutenticadoId.Value != usuarioId)
                return Prohibido("No puede consultar notificaciones de otro usuario.");
        }

        var lista = await _notificacionService
            .ObtenerNotificacionesPorUsuarioAsync(usuarioId);

        return Ok(lista);
    }

    [HttpGet("no-leidas")]
    public async Task<IActionResult> ObtenerNoLeidas()
    {
        if (UsuarioEsAdministrador())
        {
            var listaAdmin = await _notificacionService.ObtenerNoLeidasAsync();
            return Ok(listaAdmin);
        }

        var usuarioId = ObtenerUsuarioIdAutenticado();

        if (usuarioId == null)
            return Unauthorized("No se pudo identificar el usuario autenticado.");

        var listaUsuario = await _notificacionService
            .ObtenerNotificacionesPorUsuarioAsync(usuarioId.Value);

        return Ok(listaUsuario.Where(n => !n.Leida).ToList());
    }

    [HttpPut("{id:int}/leer")]
    public async Task<IActionResult> MarcarComoLeida(int id)
    {
        if (!UsuarioEsAdministrador())
        {
            var usuarioId = ObtenerUsuarioIdAutenticado();

            if (usuarioId == null)
                return Unauthorized("No se pudo identificar el usuario autenticado.");

            var notificacionesUsuario = await _notificacionService
                .ObtenerNotificacionesPorUsuarioAsync(usuarioId.Value);

            var puedeMarcar = notificacionesUsuario.Any(n => n.Id == id);

            if (!puedeMarcar)
                return Prohibido("No puede modificar notificaciones de otro usuario.");
        }

        var actualizado = await _notificacionService.MarcarComoLeidaAsync(id);

        if (!actualizado)
            return NotFound("Notificación no encontrada.");

        return Ok(new
        {
            mensaje = "Notificación marcada como leída."
        });
    }

    [Authorize(Roles = "Administrador")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> EliminarNotificacion(int id)
    {
        var eliminado = await _notificacionService.EliminarNotificacionAsync(id);

        if (!eliminado)
            return NotFound("Notificación no encontrada.");

        return NoContent();
    }
}
