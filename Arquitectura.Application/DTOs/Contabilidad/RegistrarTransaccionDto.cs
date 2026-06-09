namespace Arquitectura.Application.DTOs.Contabilidad;

public class RegistrarTransaccionDto
{
    public int CategoriaId { get; set; }

    public decimal Monto { get; set; }

    public string? Descripcion { get; set; }

    public DateTime Fecha { get; set; }
}