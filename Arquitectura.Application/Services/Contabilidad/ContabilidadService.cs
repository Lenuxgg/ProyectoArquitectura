using Arquitectura.Application.Interfaces.Contabilidad;
using Arquitectura.Infrastructure.Data;

namespace Arquitectura.Application.Services.Contabilidad;

public partial class ContabilidadService : IContabilidadService
{
    private readonly ArquitecturaDbContext _context;

    public ContabilidadService(ArquitecturaDbContext context)
    {
        _context = context;
    }
}
