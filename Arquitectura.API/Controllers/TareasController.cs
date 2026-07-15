using Arquitectura.Application.DTOs.Seguimiento;
using Arquitectura.Application.Interfaces.Seguimiento;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Arquitectura.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TareasController : ControllerBase
{
    private readonly ITareaService _tareaService;
    private readonly IWebHostEnvironment _environment;

    public TareasController(
        ITareaService tareaService,
        IWebHostEnvironment environment)
    {
        _tareaService = tareaService;
        _environment = environment;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodas()
    {
        var tareas = await _tareaService.ObtenerTodasAsync();
        return Ok(tareas);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var tarea = await _tareaService.ObtenerPorIdAsync(id);

        if (tarea == null)
            return NotFound("No se encontró la tarea.");

        return Ok(tarea);
    }

    [HttpGet("proyecto/{proyectoId}")]
    public async Task<IActionResult> ObtenerPorProyecto(int proyectoId)
    {
        var tareas = await _tareaService.ObtenerPorProyectoAsync(proyectoId);
        return Ok(tareas);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearTareaDto dto)
    {
        var tarea = await _tareaService.CrearAsync(dto);
        return Ok(tarea);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Editar(int id, [FromBody] EditarTareaDto dto)
    {
        var tarea = await _tareaService.EditarAsync(id, dto);

        if (tarea == null)
            return NotFound("No se encontró la tarea.");

        return Ok(tarea);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var resultado = await _tareaService.EliminarAsync(id);

        if (!resultado)
            return NotFound("No se encontró la tarea.");

        return Ok("Tarea eliminada correctamente.");
    }

    [HttpPut("{id}/terminar")]
    public async Task<IActionResult> MarcarComoTerminada(int id)
    {
        var resultado = await _tareaService.MarcarComoTerminadaAsync(id);

        if (!resultado)
            return NotFound("No se encontró la tarea.");

        return Ok("Tarea marcada como terminada correctamente.");
    }

    [HttpPost("asignar-empleado")]
    public async Task<IActionResult> AsignarEmpleado([FromBody] AsignarEmpleadoTareaDto dto)
    {
        var resultado = await _tareaService.AsignarEmpleadoAsync(dto);

        if (!resultado)
            return BadRequest("No se pudo asignar el empleado a la tarea. Verifique que la tarea y el usuario existan, o que no esté asignado previamente.");

        return Ok("Empleado asignado a la tarea correctamente.");
    }

    [HttpPut("asignacion/{asignacionId}")]
    public async Task<IActionResult> EditarAsignacion(int asignacionId, [FromBody] EditarAsignacionTareaDto dto)
    {
        var resultado = await _tareaService.EditarAsignacionEmpleadoAsync(asignacionId, dto);

        if (!resultado)
            return NotFound("No se encontró la asignación o el usuario indicado no existe.");

        return Ok("Asignación editada correctamente.");
    }

    [HttpDelete("asignacion/{asignacionId}")]
    public async Task<IActionResult> EliminarAsignacion(int asignacionId)
    {
        var resultado = await _tareaService.EliminarAsignacionEmpleadoAsync(asignacionId);

        if (!resultado)
            return NotFound("No se encontró la asignación.");

        return Ok("Asignación eliminada correctamente.");
    }

    [HttpPost("{tareaId:int}/documentos")]
    public async Task<IActionResult> AdjuntarDocumentoTarea(int tareaId, [FromForm] IFormFile archivo)
    {
        if (archivo == null || archivo.Length == 0)
            return BadRequest("Debe adjuntar un archivo.");

        var extensionesPermitidas = new[]
        {
            ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx", ".xlsx", ".txt"
        };

        var extension = Path.GetExtension(archivo.FileName).ToLower();

        if (!extensionesPermitidas.Contains(extension))
            return BadRequest("Tipo de archivo no permitido.");

        var rutaArchivo = await GuardarArchivoTareaAsync(archivo, "tareas");
        var usuarioId = ObtenerUsuarioIdAutenticado() ?? 1;

        var documento = await _tareaService.AdjuntarDocumentoTareaAsync(
            tareaId,
            usuarioId,
            archivo.FileName,
            rutaArchivo
        );

        if (documento == null)
            return NotFound("No se encontró la tarea o el usuario para registrar el documento.");

        return Ok(documento);
    }

    [HttpGet("{tareaId:int}/documentos")]
    public async Task<IActionResult> ObtenerDocumentosPorTarea(int tareaId)
    {
        var documentos = await _tareaService.ObtenerDocumentosPorTareaAsync(tareaId);
        return Ok(documentos);
    }

    [HttpDelete("documentos/{documentoId:int}")]
    public async Task<IActionResult> EliminarDocumentoTarea(int documentoId)
    {
        var resultado = await _tareaService.EliminarDocumentoTareaAsync(documentoId);

        if (!resultado)
            return NotFound("No se encontró el documento de la tarea.");

        return Ok("Documento eliminado correctamente.");
    }

    private async Task<string> GuardarArchivoTareaAsync(IFormFile archivo, string carpeta)
    {
        var carpetaUploads = Path.Combine(_environment.WebRootPath ?? "wwwroot", "uploads", carpeta);

        if (!Directory.Exists(carpetaUploads))
            Directory.CreateDirectory(carpetaUploads);

        var nombreArchivo = $"{Guid.NewGuid()}{Path.GetExtension(archivo.FileName)}";
        var rutaCompleta = Path.Combine(carpetaUploads, nombreArchivo);

        using (var stream = new FileStream(rutaCompleta, FileMode.Create))
        {
            await archivo.CopyToAsync(stream);
        }

        return $"/uploads/{carpeta}/{nombreArchivo}";
    }

    private int? ObtenerUsuarioIdAutenticado()
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            return null;

        return usuarioId;
    }
}
