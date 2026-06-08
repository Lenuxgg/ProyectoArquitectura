namespace Arquitectura.Domain.Entities;

public class UserRoles
{
    public int UserId { get; set; }
    public int RolesId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public Roles Roles { get; set; } = null!;
}