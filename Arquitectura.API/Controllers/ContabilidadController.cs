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

    [HttpGet("ingresos")]
    public async Task<IActionResult> ObtenerIngresos()
    {
        var lista = await _contabilidadService.ObtenerIngresosAsync();
        return Ok(lista);
    }

    [HttpGet("egresos")]
    public async Task<IActionResult> ObtenerEgresos()
    {
        var lista = await _contabilidadService.ObtenerEgresosAsync();
        return Ok(lista);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> EliminarTransaccion(int id)
    {
        var eliminado = await _contabilidadService.EliminarTransaccionAsync(id);

        if (!eliminado)
            return NotFound("Transacción no encontrada.");

        return NoContent();
    }
}