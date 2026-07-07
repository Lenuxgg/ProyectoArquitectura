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

    public async Task<ValidacionNominaDto> RevisarInconsistenciasNominaAsync(int anio, int mes)
    {
        ValidarPeriodoNomina(anio, mes);

        var periodoInicio = new DateTime(anio, mes, 1);
        var periodoFin = periodoInicio.AddMonths(1).AddDays(-1);

        var inconsistencias = new List<InconsistenciaNominaDto>();

        var existeNomina = await _context.Nomina
            .AnyAsync(n =>
                n.PeriodoInicio == periodoInicio &&
                n.PeriodoFin == periodoFin);

        if (existeNomina)
        {
            inconsistencias.Add(new InconsistenciaNominaDto
            {
                Tipo = "Periodo duplicado",
                Detalle = $"Ya existe una nómina registrada para el periodo {periodoInicio:yyyy-MM-dd} al {periodoFin:yyyy-MM-dd}."
            });
        }

        var empleadosActivos = await _context.Usuario
            .Where(u => u.Estado == "Activo")
            .ToListAsync();

        if (!empleadosActivos.Any())
        {
            inconsistencias.Add(new InconsistenciaNominaDto
            {
                Tipo = "Sin empleados",
                Detalle = "No existen empleados activos para procesar la nómina."
            });
        }

        foreach (var empleado in empleadosActivos)
        {
            var nombreEmpleado = $"{empleado.Nombre} {empleado.Apellidos}".Trim();

            if (empleado.Salario == null || empleado.Salario <= 0)
            {
                inconsistencias.Add(new InconsistenciaNominaDto
                {
                    Tipo = "Salario inválido",
                    UsuarioId = empleado.Id,
                    NombreEmpleado = nombreEmpleado,
                    Detalle = "El empleado no tiene salario registrado o el salario es menor o igual a cero."
                });
            }

            if (string.IsNullOrWhiteSpace(empleado.Puesto))
            {
                inconsistencias.Add(new InconsistenciaNominaDto
                {
                    Tipo = "Puesto faltante",
                    UsuarioId = empleado.Id,
                    NombreEmpleado = nombreEmpleado,
                    Detalle = "El empleado no tiene puesto registrado."
                });
            }

            if (string.IsNullOrWhiteSpace(empleado.Email))
            {
                inconsistencias.Add(new InconsistenciaNominaDto
                {
                    Tipo = "Correo faltante",
                    UsuarioId = empleado.Id,
                    NombreEmpleado = nombreEmpleado,
                    Detalle = "El empleado no tiene correo electrónico registrado."
                });
            }
        }

        var correosDuplicados = empleadosActivos
            .Where(u => !string.IsNullOrWhiteSpace(u.Email))
            .GroupBy(u => u.Email.Trim().ToLower())
            .Where(g => g.Count() > 1)
            .ToList();

        foreach (var grupo in correosDuplicados)
        {
            inconsistencias.Add(new InconsistenciaNominaDto
            {
                Tipo = "Empleado duplicado",
                Detalle = $"Existen varios empleados activos con el correo {grupo.Key}."
            });
        }

        return new ValidacionNominaDto
        {
            PeriodoInicio = periodoInicio,
            PeriodoFin = periodoFin,
            TieneInconsistencias = inconsistencias.Any(),
            TotalInconsistencias = inconsistencias.Count,
            Inconsistencias = inconsistencias
        };
    }

    public async Task<NominaResultadoDto> ProcesarNominaAsync(
        ProcesarNominaDto dto,
        int usuarioId)
    {
        ValidarPeriodoNomina(dto.Anio, dto.Mes);

        if (dto.BonificacionGeneral < 0)
            throw new Exception("La bonificación general no puede ser negativa.");

        var validacion = await RevisarInconsistenciasNominaAsync(dto.Anio, dto.Mes);

        if (validacion.TieneInconsistencias)
            throw new Exception("No se puede procesar la nómina porque existen inconsistencias. Revise primero el endpoint de inconsistencias.");

        var periodoInicio = new DateTime(dto.Anio, dto.Mes, 1);
        var periodoFin = periodoInicio.AddMonths(1).AddDays(-1);

        var porcentajeDeduccion = dto.PorcentajeDeduccion;

        if (porcentajeDeduccion < 0)
            throw new Exception("El porcentaje de deducción no puede ser negativo.");

        if (porcentajeDeduccion > 1)
            porcentajeDeduccion = porcentajeDeduccion / 100;

        if (porcentajeDeduccion > 1)
            throw new Exception("El porcentaje de deducción no puede ser mayor al 100%.");

        var empleados = await _context.Usuario
            .Where(u =>
                u.Estado == "Activo" &&
                u.Salario != null &&
                u.Salario > 0)
            .OrderBy(u => u.Nombre)
            .ThenBy(u => u.Apellidos)
            .ToListAsync();

        var nomina = new Nomina
        {
            PeriodoInicio = periodoInicio,
            PeriodoFin = periodoFin,
            Estado = "Procesada",
            UsuarioId = usuarioId,
            FechaRegistro = DateTime.Now,
            FechaAprobacion = DateTime.Now
        };

        foreach (var empleado in empleados)
        {
            var salarioBase = empleado.Salario!.Value;
            var deducciones = Math.Round(salarioBase * porcentajeDeduccion, 2);
            var bonificaciones = dto.BonificacionGeneral;
            var salarioNeto = salarioBase - deducciones + bonificaciones;

            nomina.Detalles.Add(new NominaDetalle
            {
                UsuarioId = empleado.Id,
                SalarioBase = salarioBase,
                Deducciones = deducciones,
                Bonificaciones = bonificaciones,
                SalarioNeto = salarioNeto,
                Inconsistencia = false,
                DetalleInconsistencia = null
            });
        }

        nomina.TotalBruto = nomina.Detalles.Sum(d => d.SalarioBase);
        nomina.TotalDeducciones = nomina.Detalles.Sum(d => d.Deducciones);
        nomina.TotalNeto = nomina.Detalles.Sum(d => d.SalarioNeto);

        _context.Nomina.Add(nomina);
        await _context.SaveChangesAsync();

        return await ObtenerNominaPorIdAsync(nomina.Id)
            ?? throw new Exception("La nómina fue procesada, pero no pudo consultarse.");
    }

    public async Task<List<NominaResultadoDto>> ObtenerNominasAsync()
    {
        return await _context.Nomina
            .Include(n => n.Detalles)
                .ThenInclude(d => d.Usuario)
            .OrderByDescending(n => n.PeriodoInicio)
            .Select(n => new NominaResultadoDto
            {
                Id = n.Id,
                PeriodoInicio = n.PeriodoInicio,
                PeriodoFin = n.PeriodoFin,
                Estado = n.Estado,
                TotalBruto = n.TotalBruto,
                TotalDeducciones = n.TotalDeducciones,
                TotalNeto = n.TotalNeto,
                UsuarioId = n.UsuarioId,
                FechaRegistro = n.FechaRegistro,
                FechaAprobacion = n.FechaAprobacion,
                Detalles = n.Detalles.Select(d => new NominaDetalleDto
                {
                    Id = d.Id,
                    UsuarioId = d.UsuarioId,
                    NombreEmpleado = d.Usuario.Nombre + " " + d.Usuario.Apellidos,
                    SalarioBase = d.SalarioBase,
                    Deducciones = d.Deducciones,
                    Bonificaciones = d.Bonificaciones,
                    SalarioNeto = d.SalarioNeto,
                    Inconsistencia = d.Inconsistencia,
                    DetalleInconsistencia = d.DetalleInconsistencia
                }).ToList()
            })
            .ToListAsync();
    }

    public async Task<NominaResultadoDto?> ObtenerNominaPorIdAsync(int id)
    {
        return await _context.Nomina
            .Include(n => n.Detalles)
                .ThenInclude(d => d.Usuario)
            .Where(n => n.Id == id)
            .Select(n => new NominaResultadoDto
            {
                Id = n.Id,
                PeriodoInicio = n.PeriodoInicio,
                PeriodoFin = n.PeriodoFin,
                Estado = n.Estado,
                TotalBruto = n.TotalBruto,
                TotalDeducciones = n.TotalDeducciones,
                TotalNeto = n.TotalNeto,
                UsuarioId = n.UsuarioId,
                FechaRegistro = n.FechaRegistro,
                FechaAprobacion = n.FechaAprobacion,
                Detalles = n.Detalles.Select(d => new NominaDetalleDto
                {
                    Id = d.Id,
                    UsuarioId = d.UsuarioId,
                    NombreEmpleado = d.Usuario.Nombre + " " + d.Usuario.Apellidos,
                    SalarioBase = d.SalarioBase,
                    Deducciones = d.Deducciones,
                    Bonificaciones = d.Bonificaciones,
                    SalarioNeto = d.SalarioNeto,
                    Inconsistencia = d.Inconsistencia,
                    DetalleInconsistencia = d.DetalleInconsistencia
                }).ToList()
            })
            .FirstOrDefaultAsync();
    }

    private static void ValidarPeriodoNomina(int anio, int mes)
    {
        if (anio < 2000 || anio > 2100)
            throw new Exception("El año de la nómina no es válido.");

        if (mes < 1 || mes > 12)
            throw new Exception("El mes de la nómina debe estar entre 1 y 12.");
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