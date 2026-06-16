using Arquitectura.Application.DTOs.Seguimiento;
using Arquitectura.Application.Interfaces.Seguimiento;
using Microsoft.AspNetCore.Mvc;

namespace Arquitectura.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ComentariosProyectosController : ControllerBase
{
    private readonly IComentarioProyectoService _comentarioProyectoService;
    private readonly IWebHostEnvironment _environment;

    public ComentariosProyectosController(
        IComentarioProyectoService comentarioProyectoService,
        IWebHostEnvironment environment)
    {
        _comentarioProyectoService = comentarioProyectoService;
        _environment = environment;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos()
    {
        var comentarios = await _comentarioProyectoService.ObtenerTodosAsync();
        return Ok(comentarios);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var comentario = await _comentarioProyectoService.ObtenerPorIdAsync(id);

        if (comentario == null)
            return NotFound("No se encontró el comentario.");

        return Ok(comentario);
    }

    [HttpGet("proyecto/{proyectoId}")]
    public async Task<IActionResult> ObtenerPorProyecto(int proyectoId)
    {
        var comentarios = await _comentarioProyectoService.ObtenerPorProyectoAsync(proyectoId);
        return Ok(comentarios);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearComentarioProyectoDto dto)
    {
        var comentario = await _comentarioProyectoService.CrearAsync(dto);
        return Ok(comentario);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var resultado = await _comentarioProyectoService.EliminarAsync(id);

        if (!resultado)
            return NotFound("No se encontró el comentario.");

        return Ok("Comentario eliminado correctamente.");
    }

    [HttpPost("{comentarioId}/imagen")]
    public async Task<IActionResult> AdjuntarImagen(int comentarioId, IFormFile archivo)
    {
        if (archivo == null || archivo.Length == 0)
            return BadRequest("Debe adjuntar una imagen.");

        var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var extension = Path.GetExtension(archivo.FileName).ToLower();

        if (!extensionesPermitidas.Contains(extension))
            return BadRequest("Solo se permiten imágenes JPG, JPEG, PNG o GIF.");

        var rutaArchivo = await GuardarArchivoAsync(archivo, "comentarios");

        var comentario = await _comentarioProyectoService.AdjuntarArchivoAsync(comentarioId, rutaArchivo);

        if (comentario == null)
            return NotFound("No se encontró el comentario.");

        return Ok(comentario);
    }

    [HttpPost("{comentarioId}/documento")]
    public async Task<IActionResult> AdjuntarDocumento(int comentarioId, IFormFile archivo)
    {
        if (archivo == null || archivo.Length == 0)
            return BadRequest("Debe adjuntar un documento.");

        var extensionesPermitidas = new[] { ".pdf", ".doc", ".docx", ".xlsx", ".txt", ".jpg", ".jpeg", ".png" };
        var extension = Path.GetExtension(archivo.FileName).ToLower();

        if (!extensionesPermitidas.Contains(extension))
            return BadRequest("Tipo de documento no permitido.");

        var rutaArchivo = await GuardarArchivoAsync(archivo, "comentarios");

        var comentario = await _comentarioProyectoService.AdjuntarArchivoAsync(comentarioId, rutaArchivo);

        if (comentario == null)
            return NotFound("No se encontró el comentario.");

        return Ok(comentario);
    }

    private async Task<string> GuardarArchivoAsync(IFormFile archivo, string carpeta)
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