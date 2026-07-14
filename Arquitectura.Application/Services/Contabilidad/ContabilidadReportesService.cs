using Arquitectura.Application.DTOs.Contabilidad;
using Microsoft.EntityFrameworkCore;

namespace Arquitectura.Application.Services.Contabilidad;

public partial class ContabilidadService
{
    public async Task<ReporteFinancieroDto> ObtenerReporteFinancieroAsync()
    {
        var ingresos = await _context.Transacciones
            .Where(t => t.Tipo == "Ingreso" && t.Activo)
            .ToListAsync();

        var egresos = await _context.Transacciones
            .Where(t => t.Tipo == "Egreso" && t.Activo)
            .ToListAsync();

        var totalIngresos = ingresos.Sum(t => t.Monto);
        var totalEgresos = egresos.Sum(t => t.Monto);

        return new ReporteFinancieroDto
        {
            TotalIngresos = totalIngresos,
            TotalEgresos = totalEgresos,
            Balance = totalIngresos - totalEgresos,
            CantidadIngresos = ingresos.Count,
            CantidadEgresos = egresos.Count
        };
    }

    public async Task<ReporteProyectoFinancieroDto?> ObtenerReportePorProyectoAsync(int proyectoId)
    {
        var proyecto = await _context.Proyectos
            .FirstOrDefaultAsync(p => p.Id == proyectoId);

        if (proyecto == null)
            return null;

        var transacciones = await _context.Transacciones
            .Include(t => t.Categoria)
            .Include(t => t.Proyecto)
            .Where(t => t.Activo && t.ProyectoId == proyectoId)
            .OrderByDescending(t => t.Fecha)
            .ThenByDescending(t => t.Id)
            .Select(t => new TransaccionDto
            {
                Id = t.Id,
                Tipo = t.Tipo,
                Monto = t.Monto,
                Descripcion = t.Descripcion,
                Fecha = t.Fecha,
                Categoria = t.Categoria.Nombre,
                UsuarioId = t.UsuarioId,
                ProyectoId = t.ProyectoId,
                ProyectoNombre = t.Proyecto != null ? t.Proyecto.Nombre : null
            })
            .ToListAsync();

        var totalIngresos = transacciones
            .Where(t => t.Tipo == "Ingreso")
            .Sum(t => t.Monto);

        var totalEgresos = transacciones
            .Where(t => t.Tipo == "Egreso")
            .Sum(t => t.Monto);

        return new ReporteProyectoFinancieroDto
        {
            ProyectoId = proyecto.Id,
            ProyectoNombre = proyecto.Nombre,
            TotalIngresos = totalIngresos,
            TotalEgresos = totalEgresos,
            Balance = totalIngresos - totalEgresos,
            CantidadIngresos = transacciones.Count(t => t.Tipo == "Ingreso"),
            CantidadEgresos = transacciones.Count(t => t.Tipo == "Egreso"),
            Transacciones = transacciones
        };
    }

    public async Task<CierreCajaDto> ObtenerCierreDiarioAsync(DateTime fecha)
    {
        var fechaInicio = fecha.Date;
        var fechaFinExclusiva = fechaInicio.AddDays(1);

        return await CalcularCierreCajaAsync(
            "Diario",
            fechaInicio,
            fechaFinExclusiva);
    }

    public async Task<CierreCajaDto> ObtenerCierreMensualAsync(int anio, int mes)
    {
        var fechaInicio = new DateTime(anio, mes, 1);
        var fechaFinExclusiva = fechaInicio.AddMonths(1);

        return await CalcularCierreCajaAsync(
            "Mensual",
            fechaInicio,
            fechaFinExclusiva);
    }

    public async Task<CierreCajaDto> ObtenerCierreAnualAsync(int anio)
    {
        var fechaInicio = new DateTime(anio, 1, 1);
        var fechaFinExclusiva = fechaInicio.AddYears(1);

        return await CalcularCierreCajaAsync(
            "Anual",
            fechaInicio,
            fechaFinExclusiva);
    }

    public async Task<CierreCajaDto> ObtenerCierrePorRangoAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        var inicio = fechaInicio.Date;
        var finExclusivo = fechaFin.Date.AddDays(1);

        if (inicio > fechaFin.Date)
            throw new Exception("La fecha de inicio no puede ser mayor que la fecha final.");

        return await CalcularCierreCajaAsync(
            "Rango de fechas",
            inicio,
            finExclusivo);
    }

    public async Task<DesgloseInformeFinancieroDto> ObtenerDesgloseInformeFinancieroAsync()
    {
        var totalIngresos = await _context.Transacciones
            .Where(t => t.Activo && t.Tipo == "Ingreso")
            .SumAsync(t => (decimal?)t.Monto) ?? 0;

        var totalEgresos = await _context.Transacciones
            .Where(t => t.Activo && t.Tipo == "Egreso")
            .SumAsync(t => (decimal?)t.Monto) ?? 0;

        var ingresosPorCategoria = await _context.Transacciones
            .Where(t => t.Activo && t.Tipo == "Ingreso")
            .GroupBy(t => t.Categoria.Nombre)
            .Select(g => new DesgloseCategoriaDto
            {
                Categoria = g.Key,
                Total = g.Sum(t => t.Monto),
                Cantidad = g.Count()
            })
            .OrderByDescending(x => x.Total)
            .ToListAsync();

        var egresosPorCategoria = await _context.Transacciones
            .Where(t => t.Activo && t.Tipo == "Egreso")
            .GroupBy(t => t.Categoria.Nombre)
            .Select(g => new DesgloseCategoriaDto
            {
                Categoria = g.Key,
                Total = g.Sum(t => t.Monto),
                Cantidad = g.Count()
            })
            .OrderByDescending(x => x.Total)
            .ToListAsync();

        var ultimasTransacciones = await _context.Transacciones
            .Include(t => t.Categoria)
            .Where(t => t.Activo)
            .OrderByDescending(t => t.Fecha)
            .ThenByDescending(t => t.Id)
            .Take(5)
            .Select(t => new TransaccionDto
            {
                Id = t.Id,
                Tipo = t.Tipo,
                Monto = t.Monto,
                Descripcion = t.Descripcion,
                Fecha = t.Fecha,
                Categoria = t.Categoria.Nombre,
                UsuarioId = t.UsuarioId
            })
            .ToListAsync();

        return new DesgloseInformeFinancieroDto
        {
            TotalIngresos = totalIngresos,
            TotalEgresos = totalEgresos,
            Balance = totalIngresos - totalEgresos,
            IngresosPorCategoria = ingresosPorCategoria,
            EgresosPorCategoria = egresosPorCategoria,
            UltimasTransacciones = ultimasTransacciones
        };
    }

    private async Task<CierreCajaDto> CalcularCierreCajaAsync(
        string tipoCierre,
        DateTime fechaInicio,
        DateTime fechaFinExclusiva)
    {
        var totalIngresos = await _context.Transacciones
            .Where(t =>
                t.Activo &&
                t.Tipo == "Ingreso" &&
                t.Fecha >= fechaInicio &&
                t.Fecha < fechaFinExclusiva)
            .SumAsync(t => (decimal?)t.Monto) ?? 0;

        var totalEgresos = await _context.Transacciones
            .Where(t =>
                t.Activo &&
                t.Tipo == "Egreso" &&
                t.Fecha >= fechaInicio &&
                t.Fecha < fechaFinExclusiva)
            .SumAsync(t => (decimal?)t.Monto) ?? 0;

        var cantidadIngresos = await _context.Transacciones
            .CountAsync(t =>
                t.Activo &&
                t.Tipo == "Ingreso" &&
                t.Fecha >= fechaInicio &&
                t.Fecha < fechaFinExclusiva);

        var cantidadEgresos = await _context.Transacciones
            .CountAsync(t =>
                t.Activo &&
                t.Tipo == "Egreso" &&
                t.Fecha >= fechaInicio &&
                t.Fecha < fechaFinExclusiva);

        return new CierreCajaDto
        {
            TipoCierre = tipoCierre,
            FechaInicio = fechaInicio,
            FechaFin = fechaFinExclusiva.AddTicks(-1),
            TotalIngresos = totalIngresos,
            TotalEgresos = totalEgresos,
            Balance = totalIngresos - totalEgresos,
            CantidadIngresos = cantidadIngresos,
            CantidadEgresos = cantidadEgresos
        };
    }
}
