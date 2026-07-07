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
    public DbSet<Nomina> Nomina => Set<Nomina>();
    public DbSet<NominaDetalle> NominaDetalle => Set<NominaDetalle>();

    // Seguimiento
    public DbSet<Proyecto> Proyectos => Set<Proyecto>();
    public DbSet<Tarea> Tareas => Set<Tarea>();
    public DbSet<ComentarioProyecto> ComentarioProyectos => Set<ComentarioProyecto>();
    public DbSet<ComentarioTarea> ComentarioTareas => Set<ComentarioTarea>();
    public DbSet<ProyectoEmpleado> ProyectoEmpleados => Set<ProyectoEmpleado>();
    public DbSet<TareaAsignacion> TareaAsignaciones => Set<TareaAsignacion>();
    public DbSet<DocumentoProyecto> DocumentoProyectos => Set<DocumentoProyecto>();

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
            e.ToTable("Usuario", tb => tb.HasTrigger("trg_Auditoria_Usuario"));
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

        modelBuilder.Entity<Nomina>(e =>
        {
            e.ToTable("Nomina");

            e.HasKey(x => x.Id);

            e.Property(x => x.PeriodoInicio)
                .HasColumnType("date")
                .IsRequired();

            e.Property(x => x.PeriodoFin)
                .HasColumnType("date")
                .IsRequired();

            e.Property(x => x.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("Pendiente")
                .IsRequired();

            e.Property(x => x.TotalBruto)
                .HasPrecision(14, 2);

            e.Property(x => x.TotalDeducciones)
                .HasPrecision(14, 2);

            e.Property(x => x.TotalNeto)
                .HasPrecision(14, 2);

            e.Property(x => x.FechaRegistro)
                .HasDefaultValueSql("GETDATE()");

            e.HasIndex(x => new { x.PeriodoInicio, x.PeriodoFin })
                .IsUnique();

            e.HasOne(x => x.Usuario)
                .WithMany()
                .HasForeignKey(x => x.UsuarioId);
        });

        modelBuilder.Entity<NominaDetalle>(e =>
        {
            e.ToTable("NominaDetalle", tb =>
            {
                tb.HasTrigger("trg_Nomina_ValidaInconsistencias");
            });

            e.HasKey(x => x.Id);

            e.Property(x => x.SalarioBase)
                .HasPrecision(12, 2);

            e.Property(x => x.Deducciones)
                .HasPrecision(12, 2);

            e.Property(x => x.Bonificaciones)
                .HasPrecision(12, 2);

            e.Property(x => x.SalarioNeto)
                .HasPrecision(12, 2);

            e.Property(x => x.DetalleInconsistencia)
                .HasMaxLength(300);

            e.HasIndex(x => new { x.NominaId, x.UsuarioId })
                .IsUnique();

            e.HasOne(x => x.Nomina)
                .WithMany(n => n.Detalles)
                .HasForeignKey(x => x.NominaId);

            e.HasOne(x => x.Usuario)
                .WithMany()
                .HasForeignKey(x => x.UsuarioId);
        });

        // ============================================================
        // MÓDULO SEGUIMIENTO
        // ============================================================

        modelBuilder.Entity<Proyecto>(e =>
        {
            e.ToTable("Proyectos", tb => tb.HasTrigger("trg_Auditoria_Proyectos"));

            e.HasKey(x => x.Id);

            e.Property(x => x.Nombre)
                .HasMaxLength(200)
                .IsRequired();

            e.Property(x => x.Descripcion);

            e.Property(x => x.FechaInicio)
                .HasColumnType("date")
                .HasDefaultValueSql("GETDATE()");

            e.Property(x => x.FechaFin)
                .HasColumnType("date");

            e.Property(x => x.Estado)
                .HasMaxLength(30)
                .HasDefaultValue("Activo")
                .IsRequired();

            e.Property(x => x.FechaCreacion)
                .HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<Tarea>(e =>
        {
            e.ToTable("Tareas", tb =>
            {
                tb.HasTrigger("trg_Auditoria_Tareas");
                tb.HasTrigger("trg_Tareas_ActualizaProyecto");
            });

            e.HasKey(x => x.Id);

            e.Property(x => x.Titulo)
                .HasMaxLength(200)
                .IsRequired();

            e.Property(x => x.Estado)
                .HasMaxLength(30)
                .HasDefaultValue("Pendiente")
                .IsRequired();

            e.Property(x => x.Descripcion);

            e.Property(x => x.FechaInicio)
                .HasColumnType("date");

            e.Property(x => x.FechaFin)
                .HasColumnType("date");

            e.Property(x => x.FechaCreacion)
                .HasDefaultValueSql("GETDATE()");

            e.HasOne(x => x.Proyecto)
                .WithMany(p => p.Tareas)
                .HasForeignKey(x => x.ProyectoId);
        });

        modelBuilder.Entity<ComentarioProyecto>(e =>
        {
            e.ToTable("ComentarioProyectos");

            e.HasKey(x => x.Id);

            e.Property(x => x.ArchivoRuta)
                .HasMaxLength(500);

            e.Property(x => x.Text)
                .IsRequired();

            e.Property(x => x.Fecha)
                .HasDefaultValueSql("GETDATE()");

            e.HasOne(x => x.Proyecto)
                .WithMany(p => p.Comentarios)
                .HasForeignKey(x => x.ProyectoId);
        });

        modelBuilder.Entity<ComentarioTarea>(e =>
        {
            e.ToTable("ComentarioTareas");

            e.HasKey(x => x.Id);

            e.Property(x => x.Texto)
                .IsRequired();

            e.Property(x => x.ArchivoRuta)
                .HasMaxLength(500);

            e.Property(x => x.Fecha)
                .HasDefaultValueSql("GETDATE()");

            e.HasOne(x => x.Tarea)
                .WithMany(t => t.Comentarios)
                .HasForeignKey(x => x.TareaId);

            e.HasOne(x => x.Usuario)
                .WithMany()
                .HasForeignKey(x => x.UsuarioId);
        });

        modelBuilder.Entity<ProyectoEmpleado>(e =>
        {
            e.ToTable("ProyectoEmpleados");

            e.HasKey(x => x.Id);

            e.Property(x => x.RolProyecto)
                .HasMaxLength(50)
                .IsRequired();

            e.Property(x => x.FechaAsignacion)
                .HasDefaultValueSql("GETDATE()");

            e.Property(x => x.Activo)
                .HasDefaultValue(true);

            e.HasOne(x => x.Proyecto)
                .WithMany(p => p.ProyectoEmpleados)
                .HasForeignKey(x => x.ProyectoId);

            e.HasOne(x => x.Usuario)
                .WithMany()
                .HasForeignKey(x => x.UsuarioId);
        });

        modelBuilder.Entity<DocumentoProyecto>(e =>
        {
            e.ToTable("DocumentoProyectos");

            e.HasKey(x => x.Id);

            e.Property(x => x.Nombre)
                .HasMaxLength(255)
                .IsRequired();

            e.Property(x => x.RutaArchivo)
                .HasMaxLength(500)
                .IsRequired();

            e.Property(x => x.FechaCarga)
                .HasDefaultValueSql("GETDATE()");

            e.HasOne(x => x.Proyecto)
                .WithMany(p => p.Documentos)
                .HasForeignKey(x => x.ProyectoId);
        });

        modelBuilder.Entity<TareaAsignacion>(e =>
        {
            e.ToTable("TareaAsignaciones");

            e.HasKey(x => x.Id);

            e.Property(x => x.FechaAsignacion)
                .HasDefaultValueSql("GETDATE()");

            e.Property(x => x.Activo)
                .HasDefaultValue(true);

            e.HasOne(x => x.Tarea)
                .WithMany(t => t.Asignaciones)
                .HasForeignKey(x => x.TareaId);

            e.HasOne(x => x.Usuario)
                .WithMany()
                .HasForeignKey(x => x.UsuarioId);
        });
    }
}