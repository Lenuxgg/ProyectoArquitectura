using Arquitectura.Application.DTOs.Notificaciones;
using Arquitectura.Application.Interfaces.Notificaciones;
using Microsoft.AspNetCore.Mvc;

namespace Arquitectura.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificacionesController : ControllerBase
{
    private readonly INotificacionService _notificacionService;

    public NotificacionesController(INotificacionService notificacionService)
    {
        _notificacionService = notificacionService;
    }

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

    [HttpGet]
    public async Task<IActionResult> ObtenerNotificaciones()
    {
        var lista = await _notificacionService.ObtenerNotificacionesAsync();
        return Ok(lista);
    }

    [HttpGet("usuario/{usuarioId:int}")]
    public async Task<IActionResult> ObtenerNotificacionesPorUsuario(int usuarioId)
    {
        var lista = await _notificacionService
            .ObtenerNotificacionesPorUsuarioAsync(usuarioId);

        return Ok(lista);
    }

    [HttpGet("no-leidas")]
    public async Task<IActionResult> ObtenerNoLeidas()
    {
        var lista = await _notificacionService.ObtenerNoLeidasAsync();
        return Ok(lista);
    }

    [HttpPut("{id:int}/leer")]
    public async Task<IActionResult> MarcarComoLeida(int id)
    {
        var actualizado = await _notificacionService.MarcarComoLeidaAsync(id);

        if (!actualizado)
            return NotFound("Notificación no encontrada.");

        return Ok(new
        {
            mensaje = "Notificación marcada como leída."
        });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> EliminarNotificacion(int id)
    {
        var eliminado = await _notificacionService.EliminarNotificacionAsync(id);

        if (!eliminado)
            return NotFound("Notificación no encontrada.");

        return NoContent();
    }
}