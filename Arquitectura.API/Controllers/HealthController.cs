using Arquitectura.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Arquitectura.API.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly ArquitecturaDbContext _context;

    public HealthController(ArquitecturaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> VerificarEstado()
    {
        try
        {
            var baseDatosDisponible = await _context.Database.CanConnectAsync();

            if (!baseDatosDisponible)
            {
                return StatusCode(503, new
                {
                    estado = "Unhealthy",
                    api = "Disponible",
                    baseDatos = "No conectada",
                    fecha = DateTime.Now
                });
            }

            return Ok(new
            {
                estado = "Healthy",
                api = "Disponible",
                baseDatos = "Conectada",
                fecha = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            return StatusCode(503, new
            {
                estado = "Unhealthy",
                api = "Disponible",
                baseDatos = "Error de conexión",
                error = ex.Message,
                fecha = DateTime.Now
            });
        }
    }
}