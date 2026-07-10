using Microsoft.AspNetCore.Mvc;

namespace Arquitectura.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompatibilidadController : ControllerBase
{

    [HttpGet("navegadores")]
    public IActionResult ObtenerNavegadoresCompatibles()
    {
        return Ok(new
        {
            requisito = "RNF-001",
            descripcion = "El sistema es accesible desde navegadores modernos.",
            navegadoresCompatibles = new[]
            {
                "Google Chrome",
                "Microsoft Edge",
                "Mozilla Firefox",
                "Safari",
                "Brave",
                "Opera",
                "Opera GX",
                "Vivaldi"
            },
            frontend = "/frontend/index.html",
            swagger = "/swagger",
            api = "Disponible",
            fechaVerificacion = DateTime.Now
        });
    }
}