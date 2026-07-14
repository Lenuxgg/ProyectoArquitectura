namespace Arquitectura.Application.DTOs.Contabilidad;

public class TransaccionDto
{
    public int Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string? Descripcion { get; set; }
    public DateTime Fecha { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public int UsuarioId { get; set; }
    public int? ProyectoId { get; set; }
    public string? ProyectoNombre { get; set; }
}