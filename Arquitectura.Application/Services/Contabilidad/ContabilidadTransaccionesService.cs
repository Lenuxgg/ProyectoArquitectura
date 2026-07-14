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
        var categoria = await _context.CategoriaFinanciera
            .FirstOrDefaultAsync(c =>
                c.Id == dto.CategoriaId &&
                c.Tipo == "Ingreso" &&
                c.Activo);

        if (categoria == null)
            throw new Exception("La categoría de ingreso no existe o está inactiva.");

        if (dto.ProyectoId.HasValue)
        {
            var proyectoExiste = await _context.Proyectos
                .AnyAsync(p => p.Id == dto.ProyectoId.Value);

            if (!proyectoExiste)
                throw new Exception("El proyecto seleccionado no existe.");
        }

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
        var categoria = await _context.CategoriaFinanciera
            .FirstOrDefaultAsync(c =>
                c.Id == dto.CategoriaId &&
                c.Tipo == "Egreso" &&
                c.Activo);

        if (categoria == null)
            throw new Exception("La categoría de egreso no existe o está inactiva.");

        if (dto.ProyectoId.HasValue)
        {
            var proyectoExiste = await _context.Proyectos
                .AnyAsync(p => p.Id == dto.ProyectoId.Value);

            if (!proyectoExiste)
                throw new Exception("El proyecto seleccionado no existe.");
        }

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

    public async Task<List<TransaccionDto>> ObtenerIngresosAsync()
    {
        return await _context.Transacciones
            .Include(t => t.Categoria)
            .Include(t => t.Proyecto)
            .Where(t => t.Tipo == "Ingreso" && t.Activo)
            .OrderByDescending(t => t.Fecha)
            .ThenByDescending(t => t.Id)
            .Select(t => new TransaccionDto
            {
                Id = t.Id,
                Tipo = t.Tipo,
                Categoria = t.Categoria.Nombre,
                Monto = t.Monto,
                Descripcion = t.Descripcion,
                Fecha = t.Fecha,
                UsuarioId = t.UsuarioId,
                ProyectoId = t.ProyectoId,
                ProyectoNombre = t.Proyecto != null ? t.Proyecto.Nombre : null
            })
            .ToListAsync();
    }

    public async Task<List<TransaccionDto>> ObtenerEgresosAsync()
    {
        return await _context.Transacciones
            .Include(t => t.Categoria)
            .Include(t => t.Proyecto)
            .Where(t => t.Tipo == "Egreso" && t.Activo)
            .OrderByDescending(t => t.Fecha)
            .ThenByDescending(t => t.Id)
            .Select(t => new TransaccionDto
            {
                Id = t.Id,
                Tipo = t.Tipo,
                Categoria = t.Categoria.Nombre,
                Monto = t.Monto,
                Descripcion = t.Descripcion,
                Fecha = t.Fecha,
                UsuarioId = t.UsuarioId,
                ProyectoId = t.ProyectoId,
                ProyectoNombre = t.Proyecto != null ? t.Proyecto.Nombre : null
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
            .Include(t => t.Proyecto)
            .Where(t => t.Activo)
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
}