using Arquitectura.Application.DTOs.Contabilidad;

namespace Arquitectura.Application.Interfaces.Contabilidad;

public interface IContabilidadService
{
    Task<int> RegistrarIngresoAsync(RegistrarTransaccionDto dto,int usuarioId);

    Task<int> RegistrarEgresoAsync(RegistrarTransaccionDto dto,int usuarioId);

    Task<List<TransaccionDto>> ObtenerIngresosAsync();

    Task<List<TransaccionDto>> ObtenerEgresosAsync();

    Task<bool> EliminarTransaccionAsync(int id);

    Task<List<TransaccionDto>> ObtenerTransaccionesAsync();
    
    Task<ReporteFinancieroDto> ObtenerReporteFinancieroAsync();

    Task<CierreCajaDto> ObtenerCierreDiarioAsync(DateTime fecha);

    Task<CierreCajaDto> ObtenerCierreMensualAsync(int anio, int mes);

    Task<CierreCajaDto> ObtenerCierreAnualAsync(int anio);

    Task<CierreCajaDto> ObtenerCierrePorRangoAsync(DateTime fechaInicio, DateTime fechaFin);
    
    Task<DesgloseInformeFinancieroDto> ObtenerDesgloseInformeFinancieroAsync();

    Task<bool> RegistrarSalarioEmpleadoAsync(int usuarioId,RegistrarSalarioEmpleadoDto dto);

    Task<ValidacionNominaDto> RevisarInconsistenciasNominaAsync(int anio, int mes);

    Task<NominaResultadoDto> ProcesarNominaAsync(ProcesarNominaDto dto, int usuarioId);

    Task<List<NominaResultadoDto>> ObtenerNominasAsync();

    Task<NominaResultadoDto?> ObtenerNominaPorIdAsync(int id);
}