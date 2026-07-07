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

    [HttpPost("ingresos")]
    public async Task<IActionResult> RegistrarIngreso(
        [FromBody] RegistrarTransaccionDto dto)
    {
        try
        {

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

    [HttpGet]
    public async Task<IActionResult> ObtenerTransacciones()
    {
        var lista = await _contabilidadService.ObtenerTransaccionesAsync();
        return Ok(lista);
    }

    [HttpGet("reporte")]
    public async Task<IActionResult> ObtenerReporteFinanciero()
    {
        var reporte = await _contabilidadService.ObtenerReporteFinancieroAsync();
        return Ok(reporte);
    }

    [HttpGet("informe/desglose")]
    public async Task<IActionResult> ObtenerDesgloseInformeFinanciero()
    {
        var informe = await _contabilidadService.ObtenerDesgloseInformeFinancieroAsync();
        return Ok(informe);
    }

    [HttpGet("cierre/diario")]
    public async Task<IActionResult> ObtenerCierreDiario([FromQuery] DateTime fecha)
    {
        var cierre = await _contabilidadService.ObtenerCierreDiarioAsync(fecha);
        return Ok(cierre);
    }

    [HttpGet("cierre/mensual")]
    public async Task<IActionResult> ObtenerCierreMensual(
        [FromQuery] int anio,
        [FromQuery] int mes)
    {
        var cierre = await _contabilidadService.ObtenerCierreMensualAsync(anio, mes);
        return Ok(cierre);
    }

    [HttpGet("cierre/anual")]
    public async Task<IActionResult> ObtenerCierreAnual([FromQuery] int anio)
    {
        var cierre = await _contabilidadService.ObtenerCierreAnualAsync(anio);
        return Ok(cierre);
    }

    [HttpGet("cierre/rango")]
    public async Task<IActionResult> ObtenerCierrePorRango(
        [FromQuery] DateTime fechaInicio,
        [FromQuery] DateTime fechaFin)
    {
        try
        {
            var cierre = await _contabilidadService.ObtenerCierrePorRangoAsync(
                fechaInicio,
                fechaFin);

            return Ok(cierre);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("empleados/{usuarioId:int}/salario")]
    public async Task<IActionResult> RegistrarSalarioEmpleado(
        int usuarioId,
        [FromBody] RegistrarSalarioEmpleadoDto dto)
    {
        try
        {
            var actualizado = await _contabilidadService
                .RegistrarSalarioEmpleadoAsync(usuarioId, dto);

            if (!actualizado)
                return NotFound("Empleado no encontrado.");

            return Ok(new
            {
                mensaje = "Salario registrado correctamente.",
                usuarioId,
                salario = dto.Salario
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

    [HttpGet("nomina/inconsistencias")]
    public async Task<IActionResult> RevisarInconsistenciasNomina(
        [FromQuery] int anio,
        [FromQuery] int mes)
    {
        try
        {
            var resultado = await _contabilidadService
                .RevisarInconsistenciasNominaAsync(anio, mes);

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                mensaje = ex.Message
            });
        }
    }

    [HttpPost("nomina/procesar")]
    public async Task<IActionResult> ProcesarNomina(
        [FromBody] ProcesarNominaDto dto)
    {
        try
        {
            int usuarioId = 1;

            var resultado = await _contabilidadService
                .ProcesarNominaAsync(dto, usuarioId);

            return Ok(new
            {
                mensaje = "Nómina procesada correctamente.",
                nomina = resultado
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

    [HttpGet("nomina")]
    public async Task<IActionResult> ObtenerNominas()
    {
        var resultado = await _contabilidadService.ObtenerNominasAsync();
        return Ok(resultado);
    }

    /// <summary>
    /// Obtiene una nómina por Id.
    /// </summary>
    [HttpGet("nomina/{id:int}")]
    public async Task<IActionResult> ObtenerNominaPorId(int id)
    {
        var resultado = await _contabilidadService.ObtenerNominaPorIdAsync(id);

        if (resultado == null)
            return NotFound("Nómina no encontrada.");

        return Ok(resultado);
    }
}