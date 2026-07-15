using Arquitectura.Application.DTOs.Contabilidad;
using Arquitectura.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Arquitectura.Application.Services.Contabilidad;

public partial class ContabilidadService
{
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
            .Where(u =>
                u.Estado == "Activo" &&
                !u.Admin)
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
                !u.Admin &&
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
}
