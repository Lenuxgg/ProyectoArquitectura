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

    /// <summary>Obtiene todos los roles del sistema.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var roles = await _service.GetAllAsync();
        return Ok(roles);
    }

    /// <summary>Obtiene un rol por su ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var rol = await _service.GetByIdAsync(id);
        return rol == null ? NotFound($"Rol con ID {id} no encontrado.") : Ok(rol);
    }

    /// <summary>Crea un nuevo rol.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearRolDto dto)
    {
        var rol = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = rol.Id }, rol);
    }

    /// <summary>Actualiza un rol existente.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ActualizarRolDto dto)
    {
        var rol = await _service.UpdateAsync(id, dto);
        return rol == null ? NotFound($"Rol con ID {id} no encontrado.") : Ok(rol);
    }

    /// <summary>Elimina un rol por su ID.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var resultado = await _service.DeleteAsync(id);
        return resultado ? NoContent() : NotFound($"Rol con ID {id} no encontrado.");
    }
}