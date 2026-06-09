using Arquitectura.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Arquitectura.Infrastructure.Data;



public class ArquitecturaDbContext : DbContext
{
    public ArquitecturaDbContext(DbContextOptions<ArquitecturaDbContext> options)
        : base(options) { }

    public DbSet<Roles> Roles => Set<Roles>();
    public DbSet<Usuario> Usuario => Set<Usuario>();
    public DbSet<UserRoles> UserRoles => Set<UserRoles>();
    public DbSet<CategoriaFinanciera> CategoriaFinanciera => Set<CategoriaFinanciera>();

public DbSet<Transaccion> Transacciones => Set<Transaccion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Roles>(e =>
        {
            e.ToTable("Roles");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("Usuario", tb => tb.HasTrigger("trg_Auditoria_Usuario")); // ← CAMBIÁ ESTA LÍNEA
            e.HasKey(x => x.Id);
            e.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
            e.Property(x => x.Apellidos).HasMaxLength(100).IsRequired();
            e.Property(x => x.Email).HasMaxLength(150).IsRequired();
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Telefono).HasMaxLength(20);
            e.Property(x => x.Puesto).HasMaxLength(100);
            e.Property(x => x.Salario).HasPrecision(12, 2);
            e.Property(x => x.Estado).HasMaxLength(20).HasDefaultValue("Activo");
            e.Property(x => x.PasswordHash).HasMaxLength(256).IsRequired();
            e.Property(x => x.FechaCreacion).HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<UserRoles>(e =>
        {
            e.ToTable("UserRoles");
            e.HasKey(x => new { x.UserId, x.RolesId });

            e.HasOne(x => x.Usuario)
             .WithMany(u => u.UserRoles)
             .HasForeignKey(x => x.UserId);

            e.HasOne(x => x.Roles)
             .WithMany(r => r.UserRoles)
             .HasForeignKey(x => x.RolesId);
        });

        modelBuilder.Entity<Transaccion>(e =>
        {
            e.ToTable("Transacciones", tb => tb.HasTrigger("trg_Auditoria_Transacciones"));

            e.HasKey(x => x.Id);

            e.Property(x => x.Tipo)
                .HasMaxLength(10)
                .IsRequired();

            e.Property(x => x.Monto)
                .HasPrecision(14, 2);

            e.Property(x => x.FechaRegistro)
                .HasDefaultValueSql("GETDATE()");

            e.HasOne(x => x.Categoria)
                .WithMany()
                .HasForeignKey(x => x.CategoriaId);

            e.HasOne(x => x.Usuario)
                .WithMany()
                .HasForeignKey(x => x.UsuarioId);
        });
    }
}