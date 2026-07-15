using System.ComponentModel.DataAnnotations;

namespace Arquitectura.Application.DTOs.Contabilidad;

public class RegistrarTransaccionDto
{
    [Range(1, int.MaxValue, ErrorMessage = "La categoría es obligatoria.")]
    public int CategoriaId { get; set; }

    [Range(0.01, 999999999999.99, ErrorMessage = "El monto debe ser mayor a 0.")]
    public decimal Monto { get; set; }

    [StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
    public string? Descripcion { get; set; }
    public DateTime Fecha { get; set; }
    public int? ProyectoId { get; set; }
}