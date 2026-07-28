using Arquitectura.Application.DTOs.Contabilidad;

namespace Arquitectura.Application.Interfaces.Contabilidad;

public interface IContabilidadService
{
    Task<int> RegistrarIngresoAsync(RegistrarTransaccionDto dto, int usuarioId);
    Task<int> RegistrarEgresoAsync(RegistrarTransaccionDto dto, int usuarioId);

    Task<List<TransaccionDto>> ObtenerIngresosAsync();
    Task<List<TransaccionDto>> ObtenerEgresosAsync();
    Task<List<TransaccionDto>> ObtenerTransaccionesAsync();

    Task<List<TransaccionDto>> ObtenerIngresosPorUsuarioAsync(int usuarioId);
    Task<List<TransaccionDto>> ObtenerEgresosPorUsuarioAsync(int usuarioId);
    Task<List<TransaccionDto>> ObtenerTransaccionesPorUsuarioAsync(int usuarioId);

    Task<bool> EliminarTransaccionAsync(int id);

    Task<ReporteFinancieroDto> ObtenerReporteFinancieroAsync();
    Task<ReporteFinancieroDto> ObtenerReporteFinancieroPorUsuarioAsync(int usuarioId);
    Task<ReporteProyectoFinancieroDto?> ObtenerReportePorProyectoAsync(int proyectoId);
    Task<bool> UsuarioTieneAccesoAProyectoAsync(int usuarioId, int proyectoId);

    Task<CierreCajaDto> ObtenerCierreDiarioAsync(DateTime fecha);
    Task<CierreCajaDto> ObtenerCierreMensualAsync(int anio, int mes);
    Task<CierreCajaDto> ObtenerCierreAnualAsync(int anio);
    Task<CierreCajaDto> ObtenerCierrePorRangoAsync(DateTime fechaInicio, DateTime fechaFin);

    Task<DesgloseInformeFinancieroDto> ObtenerDesgloseInformeFinancieroAsync();

    Task<bool> RegistrarSalarioEmpleadoAsync(int usuarioId, RegistrarSalarioEmpleadoDto dto);

    Task<ValidacionNominaDto> RevisarInconsistenciasNominaAsync(int anio, int mes);
    Task<NominaResultadoDto> ProcesarNominaAsync(ProcesarNominaDto dto, int usuarioId);
    Task<List<NominaResultadoDto>> ObtenerNominasAsync();
    Task<NominaResultadoDto?> ObtenerNominaPorIdAsync(int id);
}
