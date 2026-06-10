using Arquitectura.Application.DTOs.Seguimiento;
using Arquitectura.Application.Interfaces.Seguimiento;
using Microsoft.AspNetCore.Mvc;

namespace Arquitectura.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TareasController : ControllerBase
{
    private readonly ITareaService _tareaService;

    public TareasController(ITareaService tareaService)
    {
        _tareaService = tareaService;
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
}
