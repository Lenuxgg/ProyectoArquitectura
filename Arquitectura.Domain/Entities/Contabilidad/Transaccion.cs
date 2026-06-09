namespace Arquitectura.Domain.Entities;

public class Transaccion
{
    public int Id { get; set; }

    public int CategoriaId { get; set; }

    public string Tipo { get; set; } = string.Empty;

    public decimal Monto { get; set; }

    public string? Descripcion { get; set; }

    public DateTime Fecha { get; set; }

    public int UsuarioId { get; set; }

    public DateTime FechaRegistro { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public CategoriaFinanciera Categoria { get; set; } = null!;

    public Usuario Usuario { get; set; } = null!;
}