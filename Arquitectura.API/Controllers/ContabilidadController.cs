using Arquitectura.Application.DTOs.Contabilidad;
using Arquitectura.Application.Interfaces.Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Arquitectura.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContabilidadController : ControllerBase
{
    private readonly IContabilidadService _contabilidadService;

    public ContabilidadController(IContabilidadService contabilidadService)
    {
        _contabilidadService = contabilidadService;
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
    [HttpPost("ingresos")]
    public async Task<IActionResult> RegistrarIngreso([FromBody] RegistrarTransaccionDto dto)
    {
        try
        {
            var usuarioId = ObtenerUsuarioIdAutenticado();

            if (usuarioId == null)
                return Unauthorized("No se pudo identificar el usuario autenticado.");

            var id = await _contabilidadService.RegistrarIngresoAsync(dto, usuarioId.Value);

            return Ok(new
            {
                mensaje = "Ingreso registrado correctamente.",
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
    [HttpPost("egresos")]
    public async Task<IActionResult> RegistrarEgreso([FromBody] RegistrarTransaccionDto dto)
    {
        try
        {
            var usuarioId = ObtenerUsuarioIdAutenticado();

            if (usuarioId == null)
                return Unauthorized("No se pudo identificar el usuario autenticado.");

            var id = await _contabilidadService.RegistrarEgresoAsync(dto, usuarioId.Value);

            return Ok(new
            {
                mensaje = "Egreso registrado correctamente.",
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
        if (UsuarioEsAdministrador())
        {
            var lista = await _contabilidadService.ObtenerIngresosAsync();
            return Ok(lista);
        }

        var usuarioId = ObtenerUsuarioIdAutenticado();

        if (usuarioId == null)
            return Unauthorized("No se pudo identificar el usuario autenticado.");

        var listaEmpleado = await _contabilidadService.ObtenerIngresosPorUsuarioAsync(usuarioId.Value);
        return Ok(listaEmpleado);
    }

    [HttpGet("egresos")]
    public async Task<IActionResult> ObtenerEgresos()
    {
        if (UsuarioEsAdministrador())
        {
            var lista = await _contabilidadService.ObtenerEgresosAsync();
            return Ok(lista);
        }

        var usuarioId = ObtenerUsuarioIdAutenticado();

        if (usuarioId == null)
            return Unauthorized("No se pudo identificar el usuario autenticado.");

        var listaEmpleado = await _contabilidadService.ObtenerEgresosPorUsuarioAsync(usuarioId.Value);
        return Ok(listaEmpleado);
    }

    [Authorize(Roles = "Administrador")]
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
        if (UsuarioEsAdministrador())
        {
            var lista = await _contabilidadService.ObtenerTransaccionesAsync();
            return Ok(lista);
        }

        var usuarioId = ObtenerUsuarioIdAutenticado();

        if (usuarioId == null)
            return Unauthorized("No se pudo identificar el usuario autenticado.");

        var listaEmpleado = await _contabilidadService.ObtenerTransaccionesPorUsuarioAsync(usuarioId.Value);
        return Ok(listaEmpleado);
    }

    [HttpGet("reporte")]
    public async Task<IActionResult> ObtenerReporteFinanciero()
    {
        if (UsuarioEsAdministrador())
        {
            var reporte = await _contabilidadService.ObtenerReporteFinancieroAsync();
            return Ok(reporte);
        }

        var usuarioId = ObtenerUsuarioIdAutenticado();

        if (usuarioId == null)
            return Unauthorized("No se pudo identificar el usuario autenticado.");

        var reporteEmpleado = await _contabilidadService.ObtenerReporteFinancieroPorUsuarioAsync(usuarioId.Value);
        return Ok(reporteEmpleado);
    }

    [HttpGet("proyecto/{proyectoId:int}")]
    public async Task<IActionResult> ObtenerReportePorProyecto(int proyectoId)
    {
        if (!UsuarioEsAdministrador())
        {
            var usuarioId = ObtenerUsuarioIdAutenticado();

            if (usuarioId == null)
                return Unauthorized("No se pudo identificar el usuario autenticado.");

            var tieneAcceso = await _contabilidadService.UsuarioTieneAccesoAProyectoAsync(
                usuarioId.Value,
                proyectoId);

            if (!tieneAcceso)
                return Prohibido("No puede consultar información financiera de un proyecto no asignado.");
        }

        var reporte = await _contabilidadService.ObtenerReportePorProyectoAsync(proyectoId);

        if (reporte == null)
            return NotFound("Proyecto no encontrado.");

        return Ok(reporte);
    }

    [Authorize(Roles = "Administrador")]
    [HttpGet("informe/desglose")]
    public async Task<IActionResult> ObtenerDesgloseInformeFinanciero()
    {
        var informe = await _contabilidadService.ObtenerDesgloseInformeFinancieroAsync();
        return Ok(informe);
    }

    [Authorize(Roles = "Administrador")]
    [HttpGet("cierre/diario")]
    public async Task<IActionResult> ObtenerCierreDiario([FromQuery] DateTime fecha)
    {
        var cierre = await _contabilidadService.ObtenerCierreDiarioAsync(fecha);
        return Ok(cierre);
    }

    [Authorize(Roles = "Administrador")]
    [HttpGet("cierre/mensual")]
    public async Task<IActionResult> ObtenerCierreMensual(
        [FromQuery] int anio,
        [FromQuery] int mes)
    {
        var cierre = await _contabilidadService.ObtenerCierreMensualAsync(anio, mes);
        return Ok(cierre);
    }

    [Authorize(Roles = "Administrador")]
    [HttpGet("cierre/anual")]
    public async Task<IActionResult> ObtenerCierreAnual([FromQuery] int anio)
    {
        var cierre = await _contabilidadService.ObtenerCierreAnualAsync(anio);
        return Ok(cierre);
    }

    [Authorize(Roles = "Administrador")]
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
            return BadRequest(new
            {
                mensaje = ex.Message
            });
        }
    }

    [Authorize(Roles = "Administrador")]
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

    [Authorize(Roles = "Administrador")]
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

    [Authorize(Roles = "Administrador")]
    [HttpPost("nomina/procesar")]
    public async Task<IActionResult> ProcesarNomina([FromBody] ProcesarNominaDto dto)
    {
        try
        {
            var usuarioId = ObtenerUsuarioIdAutenticado();

            if (usuarioId == null)
                return Unauthorized("No se pudo identificar el usuario autenticado.");

            var resultado = await _contabilidadService
                .ProcesarNominaAsync(dto, usuarioId.Value);

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

    [Authorize(Roles = "Administrador")]
    [HttpGet("nomina")]
    public async Task<IActionResult> ObtenerNominas()
    {
        var resultado = await _contabilidadService.ObtenerNominasAsync();
        return Ok(resultado);
    }

    [Authorize(Roles = "Administrador")]
    [HttpGet("nomina/{id:int}")]
    public async Task<IActionResult> ObtenerNominaPorId(int id)
    {
        var resultado = await _contabilidadService.ObtenerNominaPorIdAsync(id);

        if (resultado == null)
            return NotFound("Nómina no encontrada.");

        return Ok(resultado);
    }
}
