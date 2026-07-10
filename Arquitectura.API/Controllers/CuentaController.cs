using Arquitectura.Application.DTOs.Cuenta;
using Arquitectura.Application.Interfaces.Cuenta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Arquitectura.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CuentaController : ControllerBase
{

    private int? ObtenerUsuarioIdAutenticado()
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            return null;

        return usuarioId;
    }

    private readonly ICuentaService _cuentaService;

    public CuentaController(ICuentaService cuentaService)
    {
        _cuentaService = cuentaService;
    }
    
    [Authorize]
    [HttpPost("solicitar-baja")]
    public async Task<IActionResult> SolicitarBajaCuenta(
        [FromBody] SolicitarBajaCuentaDto dto)
    {
        try
        {
            var usuarioId = ObtenerUsuarioIdAutenticado();

            if (usuarioId == null)
                return Unauthorized("No se pudo identificar el usuario autenticado.");

            var procesada = await _cuentaService
                .SolicitarBajaCuentaAsync(usuarioId.Value, dto);

            if (!procesada)
                return NotFound("La cuenta no existe o ya se encuentra dada de baja.");

            return Ok(new
            {
                mensaje = "La solicitud de baja fue procesada correctamente.",
                usuarioId = usuarioId.Value
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
}