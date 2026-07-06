namespace Arquitectura.Application.DTOs.Contabilidad;

public class DesgloseInformeFinancieroDto
{
    public decimal TotalIngresos { get; set; }

    public decimal TotalEgresos { get; set; }

    public decimal Balance { get; set; }

    public List<DesgloseCategoriaDto> IngresosPorCategoria { get; set; } = new();

    public List<DesgloseCategoriaDto> EgresosPorCategoria { get; set; } = new();

    public List<TransaccionDto> UltimasTransacciones { get; set; } = new();
}

public class DesgloseCategoriaDto
{
    public string Categoria { get; set; } = string.Empty;

    public decimal Total { get; set; }

    public int Cantidad { get; set; }
}