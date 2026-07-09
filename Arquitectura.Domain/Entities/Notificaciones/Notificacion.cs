namespace Arquitectura.Domain.Entities;

public class Notificacion
{
    public int Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Mensaje { get; set; } = string.Empty;

    public string Tipo { get; set; } = "Info";

    public int? UsuarioId { get; set; }

    public bool Leida { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public DateTime? FechaLectura { get; set; }

    public Usuario? Usuario { get; set; }
}