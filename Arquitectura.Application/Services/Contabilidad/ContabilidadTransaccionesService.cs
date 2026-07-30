using Arquitectura.Application.DTOs.Contabilidad;
using Arquitectura.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Arquitectura.Application.Services.Contabilidad;

public partial class ContabilidadService
{
    public async Task<int> RegistrarIngresoAsync(
        RegistrarTransaccionDto dto,
        int usuarioId)
    {
        await ValidarUsuarioActivoAsync(usuarioId);

        var categoria = await ObtenerCategoriaValidaAsync(dto.CategoriaId, "Ingreso");

        if (categoria == null)
            throw new Exception("La categoría de ingreso no existe o está inactiva.");

        if (dto.ProyectoId.HasValue)
            await ValidarProyectoExisteAsync(dto.ProyectoId.Value);

        var transaccion = new Transaccion
        {
            CategoriaId = dto.CategoriaId,
            Tipo = "Ingreso",
            Monto = dto.Monto,
            Descripcion = dto.Descripcion,
            Fecha = dto.Fecha == default ? DateTime.Today : dto.Fecha,
            UsuarioId = usuarioId,
            ProyectoId = dto.ProyectoId,
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
        await ValidarUsuarioActivoAsync(usuarioId);

        var categoria = await ObtenerCategoriaValidaAsync(dto.CategoriaId, "Egreso");

        if (categoria == null)
            throw new Exception("La categoría de egreso no existe o está inactiva.");

        if (dto.ProyectoId.HasValue)
            await ValidarProyectoExisteAsync(dto.ProyectoId.Value);

        var transaccion = new Transaccion
        {
            CategoriaId = dto.CategoriaId,
            Tipo = "Egreso",
            Monto = dto.Monto,
            Descripcion = dto.Descripcion,
            Fecha = dto.Fecha == default ? DateTime.Today : dto.Fecha,
            UsuarioId = usuarioId,
            ProyectoId = dto.ProyectoId,
            FechaRegistro = DateTime.Now,
            Activo = true
        };

        _context.Transacciones.Add(transaccion);
        await _context.SaveChangesAsync();

        return transaccion.Id;
    }

    public async Task<bool> ActualizarTransaccionAsync(
        int id,
        RegistrarTransaccionDto dto,
        int usuarioId,
        bool esAdministrador)
    {
        await ValidarUsuarioActivoAsync(usuarioId);

        var transaccion = await _context.Transacciones
            .FirstOrDefaultAsync(t => t.Id == id && t.Activo);

        if (transaccion == null)
            return false;

        if (!esAdministrador)
        {
            if (!dto.ProyectoId.HasValue)
                throw new UnauthorizedAccessException("Debe seleccionar un proyecto asignado para editar la transacción.");

            var tieneAcceso = await UsuarioTieneAccesoAProyectoAsync(usuarioId, dto.ProyectoId.Value);

            if (!tieneAcceso)
                throw new UnauthorizedAccessException("No tiene permiso para editar transacciones de ese proyecto.");
        }

        if (dto.ProyectoId.HasValue)
            await ValidarProyectoExisteAsync(dto.ProyectoId.Value);

        var categoria = await ObtenerCategoriaValidaAsync(dto.CategoriaId, transaccion.Tipo);

        if (categoria == null)
            throw new Exception($"La categoría de {transaccion.Tipo.ToLower()} no existe o está inactiva.");

        transaccion.CategoriaId = dto.CategoriaId;
        transaccion.Monto = dto.Monto;
        transaccion.Descripcion = dto.Descripcion;
        transaccion.Fecha = dto.Fecha == default ? DateTime.Today : dto.Fecha;
        transaccion.ProyectoId = dto.ProyectoId;
        transaccion.FechaModificacion = DateTime.Now;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<TransaccionDto>> ObtenerIngresosAsync()
    {
        return await CrearQueryBaseTransacciones()
            .Where(t => t.Tipo == "Ingreso")
            .OrderByDescending(t => t.Fecha)
            .ThenByDescending(t => t.Id)
            .Select(t => MapearTransaccionDto(t))
            .ToListAsync();
    }

    public async Task<List<TransaccionDto>> ObtenerEgresosAsync()
    {
        return await CrearQueryBaseTransacciones()
            .Where(t => t.Tipo == "Egreso")
            .OrderByDescending(t => t.Fecha)
            .ThenByDescending(t => t.Id)
            .Select(t => MapearTransaccionDto(t))
            .ToListAsync();
    }

    public async Task<List<TransaccionDto>> ObtenerTransaccionesAsync()
    {
        return await CrearQueryBaseTransacciones()
            .OrderByDescending(t => t.Fecha)
            .ThenByDescending(t => t.Id)
            .Select(t => MapearTransaccionDto(t))
            .ToListAsync();
    }

    public async Task<List<TransaccionDto>> ObtenerIngresosPorUsuarioAsync(int usuarioId)
    {
        return await CrearQueryTransaccionesPorUsuario(usuarioId)
            .Where(t => t.Tipo == "Ingreso")
            .OrderByDescending(t => t.Fecha)
            .ThenByDescending(t => t.Id)
            .Select(t => MapearTransaccionDto(t))
            .ToListAsync();
    }

    public async Task<List<TransaccionDto>> ObtenerEgresosPorUsuarioAsync(int usuarioId)
    {
        return await CrearQueryTransaccionesPorUsuario(usuarioId)
            .Where(t => t.Tipo == "Egreso")
            .OrderByDescending(t => t.Fecha)
            .ThenByDescending(t => t.Id)
            .Select(t => MapearTransaccionDto(t))
            .ToListAsync();
    }

    public async Task<List<TransaccionDto>> ObtenerTransaccionesPorUsuarioAsync(int usuarioId)
    {
        return await CrearQueryTransaccionesPorUsuario(usuarioId)
            .OrderByDescending(t => t.Fecha)
            .ThenByDescending(t => t.Id)
            .Select(t => MapearTransaccionDto(t))
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

    public async Task<bool> RegistrarSalarioEmpleadoAsync(
        int usuarioId,
        RegistrarSalarioEmpleadoDto dto)
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

    private async Task ValidarUsuarioActivoAsync(int usuarioId)
    {
        var usuarioExiste = await _context.Usuario
            .AnyAsync(u => u.Id == usuarioId && u.Estado != "Baja");

        if (!usuarioExiste)
            throw new Exception("El usuario autenticado no existe o está dado de baja. Inicie sesión nuevamente.");
    }

    private async Task<CategoriaFinanciera?> ObtenerCategoriaValidaAsync(int categoriaId, string tipo)
    {
        return await _context.CategoriaFinanciera
            .FirstOrDefaultAsync(c =>
                c.Id == categoriaId &&
                c.Tipo == tipo &&
                c.Activo);
    }

    private async Task ValidarProyectoExisteAsync(int proyectoId)
    {
        var proyectoExiste = await _context.Proyectos
            .AnyAsync(p => p.Id == proyectoId);

        if (!proyectoExiste)
            throw new Exception("El proyecto seleccionado no existe.");
    }

    private IQueryable<Transaccion> CrearQueryBaseTransacciones()
    {
        return _context.Transacciones
            .Include(t => t.Categoria)
            .Include(t => t.Proyecto)
            .Where(t => t.Activo);
    }

    private IQueryable<Transaccion> CrearQueryTransaccionesPorUsuario(int usuarioId)
    {
        var proyectosAsignados = _context.ProyectoEmpleados
            .Where(pe =>
                pe.UsuarioId == usuarioId &&
                pe.Activo)
            .Select(pe => pe.ProyectoId);

        return CrearQueryBaseTransacciones()
            .Where(t =>
                t.ProyectoId.HasValue &&
                proyectosAsignados.Contains(t.ProyectoId.Value));
    }

    private static TransaccionDto MapearTransaccionDto(Transaccion t)
    {
        return new TransaccionDto
        {
            Id = t.Id,
            Tipo = t.Tipo,
            CategoriaId = t.CategoriaId,
            Categoria = t.Categoria.Nombre,
            Monto = t.Monto,
            Descripcion = t.Descripcion,
            Fecha = t.Fecha,
            UsuarioId = t.UsuarioId,
            ProyectoId = t.ProyectoId,
            ProyectoNombre = t.Proyecto != null ? t.Proyecto.Nombre : null
        };
    }
}
