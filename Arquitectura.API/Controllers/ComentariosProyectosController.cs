using Arquitectura.Application.DTOs.Seguimiento;
using Arquitectura.Application.Interfaces.Seguimiento;
using Microsoft.AspNetCore.Mvc;

namespace Arquitectura.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ComentariosProyectosController : ControllerBase
{
    private readonly IComentarioProyectoService _comentarioProyectoService;

    public ComentariosProyectosController(IComentarioProyectoService comentarioProyectoService)
    {
        _comentarioProyectoService = comentarioProyectoService;
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
}
