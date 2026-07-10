using Arquitectura.Application.DTOs.Cuenta;
using Arquitectura.Application.Interfaces.Cuenta;
using Microsoft.AspNetCore.Mvc;

namespace Arquitectura.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CuentaController : ControllerBase
{
    private readonly ICuentaService _cuentaService;

    public CuentaController(ICuentaService cuentaService)
    {
        _cuentaService = cuentaService;
    }
    
    [HttpPost("{usuarioId:int}/solicitar-baja")]
    public async Task<IActionResult> SolicitarBajaCuenta(
        int usuarioId,
        [FromBody] SolicitarBajaCuentaDto dto)
    {
        try
        {
            var procesada = await _cuentaService
                .SolicitarBajaCuentaAsync(usuarioId, dto);

            if (!procesada)
                return NotFound("La cuenta no existe o ya se encuentra dada de baja.");

            return Ok(new
            {
                mensaje = "La solicitud de baja fue procesada correctamente.",
                usuarioId
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