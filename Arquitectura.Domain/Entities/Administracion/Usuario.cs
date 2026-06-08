namespace Arquitectura.Domain.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; } = false;
    public string? Telefono { get; set; }
    public string? Puesto { get; set; }
    public decimal? Salario { get; set; }
    public DateTime? FechaContratacion { get; set; }
    public string Estado { get; set; } = "Activo";
    public bool Admin { get; set; } = false;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaModificacion { get; set; }
    public ICollection<UserRoles> UserRoles { get; set; } = new List<UserRoles>();
}