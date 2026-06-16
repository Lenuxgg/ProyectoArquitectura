using Arquitectura.Application.DTOs.Administracion;
using Arquitectura.Application.DTOs.Auth;
using Arquitectura.Application.Interfaces.Administracion;
using Arquitectura.Application.Interfaces.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Arquitectura.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _service;
    private readonly IAuthService _authService;

    public UsuariosController(
        IUsuarioService service,
        IAuthService authService)
    {
        _service = service;
        _authService = authService;
    }

    // GET: api/Usuarios
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var lista = await _service.GetAllAsync();
        return Ok(lista);
    }

    // GET: api/Usuarios/5
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var usuario = await _service.GetByIdAsync(id);

        if (usuario == null)
            return NotFound($"Usuario con ID {id} no encontrado.");

        return Ok(usuario);
    }

    // GET: api/Usuarios/buscar?termino=jeremy
    [HttpGet("buscar")]
    public async Task<IActionResult> Buscar([FromQuery] string termino)
    {
        if (string.IsNullOrWhiteSpace(termino))
            return BadRequest("El término de búsqueda no puede estar vacío.");

        var lista = await _service.BuscarAsync(termino);
        return Ok(lista);
    }

    // POST: api/Usuarios
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearUsuarioDto dto)
    {
        var usuario = await _service.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = usuario.Id },
            usuario
        );
    }

    // PUT: api/Usuarios/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] ActualizarUsuarioDto dto)
    {
        var usuario = await _service.UpdateAsync(id, dto);

        if (usuario == null)
            return NotFound($"Usuario con ID {id} no encontrado.");

        return Ok(usuario);
    }

    // DELETE: api/Usuarios/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var resultado = await _service.DeleteAsync(id);

        if (!resultado)
            return NotFound($"Usuario con ID {id} no encontrado.");

        return NoContent();
    }

    // POST: api/Usuarios/login
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] Arquitectura.Application.DTOs.Auth.LoginDto dto)
    {
        var response = await _authService.LoginAsync(dto);

        if (response == null)
            return Unauthorized("Credenciales inválidas.");

        return Ok(response);
    }

    // POST: api/Usuarios/asignar-rol
    [HttpPost("asignar-rol")]
    public async Task<IActionResult> AsignarRol(
        [FromBody] AsignarRolDto dto)
    {
        var resultado = await _service.AsignarRolAsync(dto);

        if (!resultado)
            return Conflict("El usuario ya tiene ese rol asignado.");

        return Ok("Rol asignado correctamente.");
    }

    // DELETE: api/Usuarios/{usuarioId}/rol/{rolId}
    [HttpDelete("{usuarioId:int}/rol/{rolId:int}")]
    public async Task<IActionResult> RemoverRol(
        int usuarioId,
        int rolId)
    {
        var resultado = await _service.RemoverRolAsync(usuarioId, rolId);

        if (!resultado)
            return NotFound("Asignación de rol no encontrada.");

        return NoContent();
    }
}