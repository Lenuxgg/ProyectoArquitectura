using Arquitectura.Application.DTOs.Contabilidad;
using Arquitectura.Application.Interfaces.Contabilidad;
using Arquitectura.Domain.Entities;
using Arquitectura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace Arquitectura.Application.Services.Contabilidad;

public class ContabilidadService : IContabilidadService
{
    private readonly ArquitecturaDbContext _context;

    public ContabilidadService(ArquitecturaDbContext context)
    {
        _context = context;
    }

    public async Task<int> RegistrarIngresoAsync(
        RegistrarTransaccionDto dto,
        int usuarioId)
    {
        var categoria = await _context.CategoriaFinanciera
            .FirstOrDefaultAsync(c =>
                c.Id == dto.CategoriaId &&
                c.Tipo == "Ingreso" &&
                c.Activo);

        if (categoria == null)
            throw new Exception("La categoría de ingreso no existe o está inactiva.");

        var transaccion = new Transaccion
        {
            CategoriaId = dto.CategoriaId,
            Tipo = "Ingreso",
            Monto = dto.Monto,
            Descripcion = dto.Descripcion,
            Fecha = dto.Fecha == default ? DateTime.Today : dto.Fecha,
            UsuarioId = usuarioId,
            FechaRegistro = DateTime.Now,
            Activo = true
        };

        _context.Transacciones.Add(transaccion);

        await _context.SaveChangesAsync();

        return transaccion.Id;
    }

    public async Task<int> RegistrarEgresoAsync(
        RegistrarTransaccionDto dto,
        int usuarioId)
    {
        var categoria = await _context.CategoriaFinanciera
            .FirstOrDefaultAsync(c =>
                c.Id == dto.CategoriaId &&
                c.Tipo == "Egreso" &&
                c.Activo);

        if (categoria == null)
            throw new Exception("La categoría de egreso no existe o está inactiva.");

        var transaccion = new Transaccion
        {
            CategoriaId = dto.CategoriaId,
            Tipo = "Egreso",
            Monto = dto.Monto,
            Descripcion = dto.Descripcion,
            Fecha = dto.Fecha == default ? DateTime.Today : dto.Fecha,
            UsuarioId = usuarioId,
            FechaRegistro = DateTime.Now,
            Activo = true
        };

        _context.Transacciones.Add(transaccion);

        await _context.SaveChangesAsync();

        return transaccion.Id;
    }

    public async Task<List<TransaccionDto>> ObtenerIngresosAsync()
    {
        return await _context.Transacciones
            .Include(t => t.Categoria)
            .Where(t => t.Tipo == "Ingreso" && t.Activo)
            .Select(t => new TransaccionDto
            {
                Id = t.Id,
                Tipo = t.Tipo,
                Categoria = t.Categoria.Nombre,
                Monto = t.Monto,
                Descripcion = t.Descripcion,
                Fecha = t.Fecha,
                UsuarioId = t.UsuarioId
            })
            .ToListAsync();
    }

    public async Task<List<TransaccionDto>> ObtenerEgresosAsync()
    {
        return await _context.Transacciones
            .Include(t => t.Categoria)
            .Where(t => t.Tipo == "Egreso" && t.Activo)
            .Select(t => new TransaccionDto
            {
                Id = t.Id,
                Tipo = t.Tipo,
                Categoria = t.Categoria.Nombre,
                Monto = t.Monto,
                Descripcion = t.Descripcion,
                Fecha = t.Fecha,
                UsuarioId = t.UsuarioId
            })
            .ToListAsync();
    }

    public async Task<bool> EliminarTransaccionAsync(int id)
    {
        var transaccion = await _context.Transacciones
            .FirstOrDefaultAsync(t => t.Id == id);

        if (transaccion == null)
            return false;

        transaccion.Activo = false;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<TransaccionDto>> ObtenerTransaccionesAsync()
    {
        return await _context.Transacciones
            .Include(t => t.Categoria)
            .Where(t => t.Activo)
            .OrderByDescending(t => t.Fecha)
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
    }

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

    public async Task<bool> RegistrarSalarioEmpleadoAsync(int usuarioId,RegistrarSalarioEmpleadoDto dto)
    {
        if (dto.Salario <= 0)
            throw new Exception("El salario debe ser mayor que cero.");

        var usuario = await _context.Usuario
            .FirstOrDefaultAsync(u =>
                u.Id == usuarioId &&
                u.Estado != "Baja");

        if (usuario == null)
            return false;

        usuario.Salario = dto.Salario;
        usuario.FechaModificacion = DateTime.Now;

        await _context.SaveChangesAsync();

        return true;
    }
}