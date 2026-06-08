using Arquitectura.Application.DTOs.Administracion;
using Arquitectura.Application.Interfaces.Administracion;
using Microsoft.AspNetCore.Mvc;

namespace Arquitectura.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _service;
    public UsuariosController(IUsuarioService service) => _service = service;

    /// <summary>Obtiene todos los usuarios activos.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var lista = await _service.GetAllAsync();
        return Ok(lista);
    }

    /// <summary>Obtiene un usuario por su ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var usuario = await _service.GetByIdAsync(id);
        return usuario == null ? NotFound($"Usuario con ID {id} no encontrado.") : Ok(usuario);
    }

    /// <summary>Busca usuarios por nombre, apellido o correo.</summary>
    [HttpGet("buscar")]
    public async Task<IActionResult> Buscar([FromQuery] string termino)
    {
        if (string.IsNullOrWhiteSpace(termino))
            return BadRequest("El término de búsqueda no puede estar vacío.");
        var lista = await _service.BuscarAsync(termino);
        return Ok(lista);
    }

    /// <summary>Registra un nuevo usuario.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearUsuarioDto dto)
    {
        var usuario = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = usuario.Id }, usuario);
    }

    /// <summary>Actualiza el perfil de un usuario existente.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ActualizarUsuarioDto dto)
    {
        var usuario = await _service.UpdateAsync(id, dto);
        return usuario == null ? NotFound($"Usuario con ID {id} no encontrado.") : Ok(usuario);
    }

    /// <summary>Da de baja a un usuario (baja lógica).</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var resultado = await _service.DeleteAsync(id);
        return resultado ? NoContent() : NotFound($"Usuario con ID {id} no encontrado.");
    }

    /// <summary>Inicia sesión con email y contraseña.</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var usuario = await _service.LoginAsync(dto);
        if (usuario == null)
            return Unauthorized("Credenciales incorrectas o usuario inactivo.");
        return Ok(usuario);
    }

    /// <summary>Asigna un rol a un usuario.</summary>
    [HttpPost("asignar-rol")]
    public async Task<IActionResult> AsignarRol([FromBody] AsignarRolDto dto)
    {
        var resultado = await _service.AsignarRolAsync(dto);
        if (!resultado)
            return Conflict("El usuario ya tiene ese rol asignado.");
        return Ok("Rol asignado correctamente.");
    }

    /// <summary>Remueve un rol de un usuario.</summary>
    [HttpDelete("{usuarioId:int}/rol/{rolId:int}")]
    public async Task<IActionResult> RemoverRol(int usuarioId, int rolId)
    {
        var resultado = await _service.RemoverRolAsync(usuarioId, rolId);
        return resultado ? NoContent() : NotFound("Asignación de rol no encontrada.");
    }
}