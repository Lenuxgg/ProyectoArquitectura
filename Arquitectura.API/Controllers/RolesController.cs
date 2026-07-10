using Arquitectura.Application.DTOs.Administracion;
using Arquitectura.Application.Interfaces.Administracion;
using Microsoft.AspNetCore.Mvc;

namespace Arquitectura.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class RolesController : ControllerBase
{
    private readonly IRolService _service;
    public RolesController(IRolService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var roles = await _service.GetAllAsync();
        return Ok(roles);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var rol = await _service.GetByIdAsync(id);
        return rol == null ? NotFound($"Rol con ID {id} no encontrado.") : Ok(rol);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearRolDto dto)
    {
        var rol = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = rol.Id }, rol);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ActualizarRolDto dto)
    {
        var rol = await _service.UpdateAsync(id, dto);
        return rol == null ? NotFound($"Rol con ID {id} no encontrado.") : Ok(rol);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var resultado = await _service.DeleteAsync(id);
        return resultado ? NoContent() : NotFound($"Rol con ID {id} no encontrado.");
    }
}