using Arquitectura.Application.Interfaces.Exportaciones;
using Microsoft.AspNetCore.Mvc;

namespace Arquitectura.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExportacionesController : ControllerBase
{
    private readonly IExportacionService _exportacionService;

    public ExportacionesController(IExportacionService exportacionService)
    {
        _exportacionService = exportacionService;
    }

    [HttpGet("usuarios")]
    public async Task<IActionResult> ExportarUsuarios([FromQuery] string formato = "csv")
    {
        try
        {
            var archivo = await _exportacionService.ExportarUsuariosAsync(formato);

            return File(
                archivo.Contenido,
                archivo.ContentType,
                archivo.NombreArchivo);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet("proyectos")]
    public async Task<IActionResult> ExportarProyectos([FromQuery] string formato = "csv")
    {
        try
        {
            var archivo = await _exportacionService.ExportarProyectosAsync(formato);

            return File(
                archivo.Contenido,
                archivo.ContentType,
                archivo.NombreArchivo);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet("tareas")]
    public async Task<IActionResult> ExportarTareas([FromQuery] string formato = "csv")
    {
        try
        {
            var archivo = await _exportacionService.ExportarTareasAsync(formato);

            return File(
                archivo.Contenido,
                archivo.ContentType,
                archivo.NombreArchivo);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet("transacciones")]
    public async Task<IActionResult> ExportarTransacciones([FromQuery] string formato = "csv")
    {
        try
        {
            var archivo = await _exportacionService.ExportarTransaccionesAsync(formato);

            return File(
                archivo.Contenido,
                archivo.ContentType,
                archivo.NombreArchivo);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet("nomina")]
    public async Task<IActionResult> ExportarNomina([FromQuery] string formato = "csv")
    {
        try
        {
            var archivo = await _exportacionService.ExportarNominaAsync(formato);

            return File(
                archivo.Contenido,
                archivo.ContentType,
                archivo.NombreArchivo);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}