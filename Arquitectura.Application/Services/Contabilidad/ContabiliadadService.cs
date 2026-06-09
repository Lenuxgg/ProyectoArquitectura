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
            Fecha = dto.Fecha,
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
            Fecha = dto.Fecha,
            UsuarioId = usuarioId,
            FechaRegistro = DateTime.Now,
            Activo = true
        };

        _context.Transacciones.Add(transaccion);

        await _context.SaveChangesAsync();

        return transaccion.Id;
    }
}