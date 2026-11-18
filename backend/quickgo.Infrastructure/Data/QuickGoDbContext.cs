using Microsoft.EntityFrameworkCore;
using quickgo.Domain.entities;
using quickgo.Domain.Enums;

namespace quickgo.Infrastructure.Data;

public class QuickGoDbContext : DbContext
{
    public QuickGoDbContext(DbContextOptions<QuickGoDbContext> options) : base(options)
    {
    }

    public DbSet<TipoVehiculo> TipoVehiculos => Set<TipoVehiculo>();
    public DbSet<Vehiculo> Vehiculos => Set<Vehiculo>();
    public DbSet<DocumentoVehiculo> DocumentosVehiculo => Set<DocumentoVehiculo>();
    public DbSet<AsignacionVehiculoConductor> AsignacionesVehiculoConductor => Set<AsignacionVehiculoConductor>();
    public DbSet<Conductor> Conductores => Set<Conductor>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Envio> Envios => Set<Envio>();
    public DbSet<Empresa> Empresas => Set<Empresa>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TipoVehiculo>(entity =>
        {
            entity.ToTable("tipos_vehiculo");
            entity.HasKey(tipo => tipo.TipoVehiculoId);
            entity.Property(tipo => tipo.Nombre).HasMaxLength(80).IsRequired();
            entity.Property(tipo => tipo.Cantidadkilogramo).HasPrecision(12, 2);
            entity.Property(tipo => tipo.LargoMetros).HasPrecision(8, 2);
            entity.Property(tipo => tipo.AnchoMetros).HasPrecision(8, 2);
            entity.Property(tipo => tipo.AlturaMetros).HasPrecision(8, 2);
            entity.Property(tipo => tipo.VolumenMaximoMetrosCubicos).HasPrecision(12, 2);
            entity.HasData(TipoVehiculoCatalogo.DatosIniciales);
        });

        modelBuilder.Entity<Vehiculo>(entity =>
        {
            entity.ToTable("vehiculos");
            entity.HasKey(vehiculo => vehiculo.VehiculoId);
            entity.Property(vehiculo => vehiculo.Placa).HasMaxLength(20).IsRequired();
            entity.Property(vehiculo => vehiculo.Vin).HasMaxLength(50);
            entity.Property(vehiculo => vehiculo.Propiedad).HasConversion<string>().HasMaxLength(20);
            entity.Property(vehiculo => vehiculo.Estado).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(vehiculo => new { vehiculo.EmpresaId, vehiculo.Placa }).IsUnique();
            entity.HasOne(vehiculo => vehiculo.TipoVehiculo)
                .WithMany(tipo => tipo.Vehiculos)
                .HasForeignKey(vehiculo => vehiculo.TipoVehiculoId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Empresa>()
                .WithMany()
                .HasForeignKey(vehiculo => vehiculo.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DocumentoVehiculo>(entity =>
        {
            entity.ToTable("documentos_vehiculo");
            entity.HasKey(documento => documento.Id);
            entity.Property(documento => documento.Tipo).HasConversion<string>().HasMaxLength(20);
            entity.Property(documento => documento.Numero).HasMaxLength(80).IsRequired();
            entity.HasOne(documento => documento.Vehiculo)
                .WithMany(vehiculo => vehiculo.Documentos)
                .HasForeignKey(documento => documento.VehiculoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuarios");
            entity.HasKey(usuario => usuario.Id);
            entity.Property(usuario => usuario.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(usuario => usuario.Apellido).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<Conductor>(entity =>
        {
            entity.ToTable("conductores");
            entity.HasKey(conductor => conductor.Id);
            entity.HasIndex(conductor => new { conductor.EmpresaId, conductor.UsuarioId }).IsUnique();
            entity.HasOne(conductor => conductor.Usuario)
                .WithOne(usuario => usuario.Conductor)
                .HasForeignKey<Conductor>(conductor => conductor.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Empresa>()
                .WithMany()
                .HasForeignKey(conductor => conductor.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AsignacionVehiculoConductor>(entity =>
        {
            entity.ToTable("asignaciones_vehiculo_conductor");
            entity.HasKey(asignacion => asignacion.Id);
            entity.HasIndex(asignacion => new { asignacion.EmpresaId, asignacion.VehiculoId })
                .IsUnique()
                .HasFilter("\"FechaFin\" IS NULL");
            entity.HasOne(asignacion => asignacion.Vehiculo)
                .WithMany(vehiculo => vehiculo.Asignaciones)
                .HasForeignKey(asignacion => asignacion.VehiculoId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(asignacion => asignacion.Conductor)
                .WithMany(conductor => conductor.Asignaciones)
                .HasForeignKey(asignacion => asignacion.ConductorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Envio>(entity =>
        {
            entity.ToTable("envios");
            entity.HasKey(envio => envio.Id);
            entity.Property(envio => envio.Estado).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(envio => new { envio.EmpresaId, envio.VehiculoId, envio.Estado });
            entity.HasOne(envio => envio.Vehiculo)
                .WithMany(vehiculo => vehiculo.Envios)
                .HasForeignKey(envio => envio.VehiculoId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}