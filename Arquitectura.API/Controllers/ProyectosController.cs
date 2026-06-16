using Arquitectura.Application.DTOs.Seguimiento;
using Arquitectura.Application.Interfaces.Seguimiento;
using Microsoft.AspNetCore.Mvc;

namespace Arquitectura.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProyectosController : ControllerBase
{
    private readonly IProyectoService _proyectoService;
    private readonly IWebHostEnvironment _environment;

    public ProyectosController(
        IProyectoService proyectoService,
        IWebHostEnvironment environment)
    {
        _proyectoService = proyectoService;
        _environment = environment;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos()
    {
        var proyectos = await _proyectoService.ObtenerTodosAsync();
        return Ok(proyectos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var proyecto = await _proyectoService.ObtenerPorIdAsync(id);

        if (proyecto == null)
            return NotFound("No se encontró el proyecto.");

        return Ok(proyecto);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearProyectoDto dto)
    {
        var proyecto = await _proyectoService.CrearAsync(dto);
        return Ok(proyecto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Editar(int id, [FromBody] EditarProyectoDto dto)
    {
        var proyecto = await _proyectoService.EditarAsync(id, dto);

        if (proyecto == null)
            return NotFound("No se encontró el proyecto.");

        return Ok(proyecto);
    }

    [HttpPut("{id}/terminar")]
    public async Task<IActionResult> TerminarProyecto(int id)
    {
        var resultado = await _proyectoService.TerminarProyectoAsync(id);

        if (!resultado)
            return NotFound("No se encontró el proyecto.");

        return Ok("Proyecto terminado correctamente.");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var resultado = await _proyectoService.EliminarAsync(id);

        if (!resultado)
            return NotFound("No se encontró el proyecto.");

        return Ok("Proyecto eliminado correctamente.");
    }

    [HttpPost("asignar-empleado")]
    public async Task<IActionResult> AsignarEmpleado([FromBody] AsignarEmpleadoProyectoDto dto)
    {
        var resultado = await _proyectoService.AsignarEmpleadoAsync(dto);

        if (!resultado)
            return BadRequest("No se pudo asignar el empleado al proyecto. Verifique que el proyecto y el usuario existan, o que no esté asignado previamente.");

        return Ok("Empleado asignado al proyecto con rol correctamente.");
    }

    [HttpPut("asignacion/{asignacionId}")]
    public async Task<IActionResult> EditarAsignacion(int asignacionId, [FromBody] EditarAsignacionProyectoDto dto)
    {
        var resultado = await _proyectoService.EditarAsignacionEmpleadoAsync(asignacionId, dto);

        if (!resultado)
            return NotFound("No se encontró la asignación o el usuario indicado no existe.");

        return Ok("Rol del empleado en el proyecto editado correctamente.");
    }

    [HttpDelete("asignacion/{asignacionId}")]
    public async Task<IActionResult> EliminarEmpleadoProyecto(int asignacionId)
    {
        var resultado = await _proyectoService.EliminarEmpleadoProyectoAsync(asignacionId);

        if (!resultado)
            return NotFound("No se encontró la asignación.");

        return Ok("Empleado eliminado del proyecto correctamente.");
    }

    [HttpGet("{proyectoId}/empleados")]
    public async Task<IActionResult> ObtenerEmpleadosPorProyecto(int proyectoId)
    {
        var empleados = await _proyectoService.ObtenerEmpleadosPorProyectoAsync(proyectoId);
        return Ok(empleados);
    }

    [HttpPost("{proyectoId}/documentos")]
    public async Task<IActionResult> AdjuntarDocumentoProyecto(int proyectoId, IFormFile archivo)
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

        var rutaArchivo = await GuardarArchivoProyectoAsync(archivo, "proyectos");

        var documento = await _proyectoService.AdjuntarDocumentoProyectoAsync(
            proyectoId,
            archivo.FileName,
            rutaArchivo
        );

        if (documento == null)
            return NotFound("No se encontró el proyecto.");

        return Ok(documento);
    }

    [HttpGet("{proyectoId}/documentos")]
    public async Task<IActionResult> ObtenerDocumentosPorProyecto(int proyectoId)
    {
        var documentos = await _proyectoService.ObtenerDocumentosPorProyectoAsync(proyectoId);
        return Ok(documentos);
    }

    [HttpDelete("documentos/{documentoId}")]
    public async Task<IActionResult> EliminarDocumentoProyecto(int documentoId)
    {
        var resultado = await _proyectoService.EliminarDocumentoProyectoAsync(documentoId);

        if (!resultado)
            return NotFound("No se encontró el documento.");

        return Ok("Documento eliminado correctamente.");
    }

    private async Task<string> GuardarArchivoProyectoAsync(IFormFile archivo, string carpeta)
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
}