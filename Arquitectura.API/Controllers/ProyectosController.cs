using Arquitectura.Application.DTOs.Seguimiento;
using Arquitectura.Application.Interfaces.Seguimiento;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Arquitectura.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
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
        if (UsuarioEsAdministrador())
        {
            var proyectos = await _proyectoService.ObtenerTodosAsync();
            return Ok(proyectos);
        }

        var usuarioId = ObtenerUsuarioIdAutenticado();

        if (usuarioId == null)
            return Unauthorized("No se pudo identificar el usuario autenticado.");

        var proyectosUsuario = await _proyectoService.ObtenerPorUsuarioAsync(usuarioId.Value);
        return Ok(proyectosUsuario);
    }

    [HttpGet("usuario/{usuarioId}")]
    public async Task<IActionResult> ObtenerPorUsuario(int usuarioId)
    {
        if (!UsuarioEsAdministrador())
        {
            var usuarioAutenticadoId = ObtenerUsuarioIdAutenticado();

            if (usuarioAutenticadoId == null)
                return Unauthorized("No se pudo identificar el usuario autenticado.");

            if (usuarioAutenticadoId.Value != usuarioId)
                return StatusCode(403, "No puede consultar proyectos de otro usuario.");
        }

        var proyectos = await _proyectoService.ObtenerPorUsuarioAsync(usuarioId);
        return Ok(proyectos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        if (!await UsuarioPuedeAccederAProyectoAsync(id))
            return StatusCode(403, "No tiene permisos para consultar este proyecto.");

        var proyecto = await _proyectoService.ObtenerPorIdAsync(id);

        if (proyecto == null)
            return NotFound("No se encontró el proyecto.");

        return Ok(proyecto);
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearProyectoDto dto)
    {
        var proyecto = await _proyectoService.CrearAsync(dto);
        return Ok(proyecto);
    }

    [Authorize(Roles = "Administrador")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Editar(int id, [FromBody] EditarProyectoDto dto)
    {
        var proyecto = await _proyectoService.EditarAsync(id, dto);

        if (proyecto == null)
            return NotFound("No se encontró el proyecto.");

        return Ok(proyecto);
    }

    [Authorize(Roles = "Administrador")]
    [HttpPut("{id}/terminar")]
    public async Task<IActionResult> TerminarProyecto(int id)
    {
        var resultado = await _proyectoService.TerminarProyectoAsync(id);

        if (!resultado)
            return NotFound("No se encontró el proyecto.");

        return Ok("Proyecto terminado correctamente.");
    }

    [Authorize(Roles = "Administrador")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var resultado = await _proyectoService.EliminarAsync(id);

        if (!resultado)
            return NotFound("No se encontró el proyecto.");

        return Ok("Proyecto eliminado correctamente.");
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost("asignar-empleado")]
    public async Task<IActionResult> AsignarEmpleado([FromBody] AsignarEmpleadoProyectoDto dto)
    {
        var resultado = await _proyectoService.AsignarEmpleadoAsync(dto);

        if (!resultado)
            return BadRequest("No se pudo asignar el empleado al proyecto. Verifique que el proyecto y el usuario existan, o que no esté asignado previamente.");

        return Ok("Empleado asignado al proyecto con rol correctamente.");
    }

    [Authorize(Roles = "Administrador")]
    [HttpPut("asignacion/{asignacionId}")]
    public async Task<IActionResult> EditarAsignacion(int asignacionId, [FromBody] EditarAsignacionProyectoDto dto)
    {
        var resultado = await _proyectoService.EditarAsignacionEmpleadoAsync(asignacionId, dto);

        if (!resultado)
            return NotFound("No se encontró la asignación o el usuario indicado no existe.");

        return Ok("Rol del empleado en el proyecto editado correctamente.");
    }

    [Authorize(Roles = "Administrador")]
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
        if (!await UsuarioPuedeAccederAProyectoAsync(proyectoId))
            return StatusCode(403, "No tiene permisos para consultar empleados de este proyecto.");

        var empleados = await _proyectoService.ObtenerEmpleadosPorProyectoAsync(proyectoId);
        return Ok(empleados);
    }

    [HttpPost("{proyectoId}/documentos")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AdjuntarDocumentoProyecto(int proyectoId, IFormFile archivo)
    {
        if (!await UsuarioPuedeAccederAProyectoAsync(proyectoId))
            return StatusCode(403, "No tiene permisos para adjuntar documentos a este proyecto.");

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
        if (!await UsuarioPuedeAccederAProyectoAsync(proyectoId))
            return StatusCode(403, "No tiene permisos para consultar documentos de este proyecto.");

        var documentos = await _proyectoService.ObtenerDocumentosPorProyectoAsync(proyectoId);
        return Ok(documentos);
    }

    [Authorize(Roles = "Administrador")]
    [HttpDelete("documentos/{documentoId}")]
    public async Task<IActionResult> EliminarDocumentoProyecto(int documentoId)
    {
        var resultado = await _proyectoService.EliminarDocumentoProyectoAsync(documentoId);

        if (!resultado)
            return NotFound("No se encontró el documento.");

        return Ok("Documento eliminado correctamente.");
    }

    private bool UsuarioEsAdministrador()
    {
        return User.IsInRole("Administrador");
    }

    private int? ObtenerUsuarioIdAutenticado()
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            return null;

        return usuarioId;
    }

    private async Task<bool> UsuarioPuedeAccederAProyectoAsync(int proyectoId)
    {
        if (UsuarioEsAdministrador())
            return true;

        var usuarioId = ObtenerUsuarioIdAutenticado();

        if (usuarioId == null)
            return false;

        var proyectosUsuario = await _proyectoService.ObtenerPorUsuarioAsync(usuarioId.Value);
        return proyectosUsuario.Any(p => p.Id == proyectoId);
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
