using Arquitectura.Application.DTOs.Exportaciones;

namespace Arquitectura.Application.Interfaces.Exportaciones;

public interface IExportacionService
{
    Task<ExportacionArchivoDto> ExportarUsuariosAsync(string formato);

    Task<ExportacionArchivoDto> ExportarProyectosAsync(string formato);

    Task<ExportacionArchivoDto> ExportarTareasAsync(string formato);

    Task<ExportacionArchivoDto> ExportarTransaccionesAsync(string formato);

    Task<ExportacionArchivoDto> ExportarNominaAsync(string formato);
}
