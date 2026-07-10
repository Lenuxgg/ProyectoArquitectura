using Arquitectura.Application.DTOs.Empleados;
using Arquitectura.Application.Interfaces.Empleados;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arquitectura.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class EmpleadosController : ControllerBase
{
    private readonly IEmpleadoService _service;
    public EmpleadosController(IEmpleadoService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var lista = await _service.GetAllAsync();
        return Ok(lista);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var empleado = await _service.GetByIdAsync(id);
        return empleado == null ? NotFound($"Empleado con ID {id} no encontrado.") : Ok(empleado);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearEmpleadoDto dto)
    {
        var empleado = await _service.CrearEmpleadoAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = empleado.Id }, empleado);
    }

    [HttpPut("{id:int}/puesto")]
    public async Task<IActionResult> ActualizarPuesto(int id, [FromBody] ActualizarPuestoDto dto)
    {
        var empleado = await _service.ActualizarPuestoAsync(id, dto);
        return empleado == null ? NotFound($"Empleado con ID {id} no encontrado.") : Ok(empleado);
    }

    [HttpPut("{id:int}/salario")]
    public async Task<IActionResult> ActualizarSalario(int id, [FromBody] ActualizarSalarioDto dto)
    {
        try
        {
            var empleado = await _service.ActualizarSalarioAsync(id, dto);
            return empleado == null ? NotFound($"Empleado con ID {id} no encontrado.") : Ok(empleado);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DarDeBaja(int id)
    {
        var resultado = await _service.DarDeBajaAsync(id);
        return resultado ? NoContent() : NotFound($"Empleado con ID {id} no encontrado.");
    }
}