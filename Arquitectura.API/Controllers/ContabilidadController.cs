using Arquitectura.Application.DTOs.Contabilidad;
using Arquitectura.Application.Interfaces.Contabilidad;
using Microsoft.AspNetCore.Mvc;

namespace Arquitectura.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContabilidadController : ControllerBase
{
    private readonly IContabilidadService _contabilidadService;

    public ContabilidadController(IContabilidadService contabilidadService)
    {
        _contabilidadService = contabilidadService;
    }

    /// <summary>
    /// CONT-001 Registrar ingreso
    /// </summary>
    [HttpPost("ingresos")]
    public async Task<IActionResult> RegistrarIngreso(
        [FromBody] RegistrarTransaccionDto dto)
    {
        try
        {
            // Temporal mientras no tengan JWT
            int usuarioId = 1;

            var id = await _contabilidadService
                .RegistrarIngresoAsync(dto, usuarioId);

            return Ok(new
            {
                mensaje = "Ingreso registrado correctamente",
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

    /// <summary>
    /// CONT-002 Registrar egreso
    /// </summary>
    [HttpPost("egresos")]
    public async Task<IActionResult> RegistrarEgreso(
        [FromBody] RegistrarTransaccionDto dto)
    {
        try
        {
            int usuarioId = 1;

            var id = await _contabilidadService
                .RegistrarEgresoAsync(dto, usuarioId);

            return Ok(new
            {
                mensaje = "Egreso registrado correctamente",
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
}