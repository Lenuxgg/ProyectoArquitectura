using Arquitectura.Application.DTOs.Administracion;
using Arquitectura.Application.Interfaces.Administracion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arquitectura.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Administrador")]
public class RolesController : ControllerBase
{
    private readonly IRolService _service;

    public RolesController(IRolService service)
    {
        _service = service;
    }

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

        if (rol == null)
            return NotFound($"Rol con ID {id} no encontrado.");

        return Ok(rol);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearRolDto dto)
    {
        var rol = await _service.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = rol.Id },
            rol);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] ActualizarRolDto dto)
    {
        var rol = await _service.UpdateAsync(id, dto);

        if (rol == null)
            return NotFound($"Rol con ID {id} no encontrado.");

        return Ok(rol);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var resultado = await _service.DeleteAsync(id);

        if (!resultado)
            return NotFound($"Rol con ID {id} no encontrado.");

        return NoContent();
    }
}
