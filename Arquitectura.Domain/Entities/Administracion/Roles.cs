namespace Arquitectura.Domain.Entities;

public class Roles
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public ICollection<UserRoles> UserRoles { get; set; } = new List<UserRoles>();
}