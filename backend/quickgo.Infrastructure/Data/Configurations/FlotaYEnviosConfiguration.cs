using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using quickgo.Domain.entities;

namespace quickgo.Infrastructure.Data.Configurations;

// Alinea la base con el ERD (columna "id", capacidad_kg, fecha_asignacion, etc.)
// sin renombrar las propiedades de C# que ya usan los controladores y repositorios.

public class TipoVehiculoConfiguration : IEntityTypeConfiguration<TipoVehiculo>
{
    public void Configure(EntityTypeBuilder<TipoVehiculo> builder)
    {
        builder.ToTable("tipos_vehiculo");
        builder.HasKey(t => t.TipoVehiculoId);
        builder.Property(t => t.TipoVehiculoId).HasColumnName("id");
        builder.Property(t => t.Nombre).HasColumnName("nombre").HasMaxLength(80).IsRequired();
        builder.HasIndex(t => t.Nombre).IsUnique();
        builder.HasData(TipoVehiculoCatalogo.DatosIniciales);
        builder.Property(t => t.Cantidadkilogramo).HasColumnName("capacidad_kg").HasPrecision(10, 2);
        builder.Property(t => t.LargoMetros).HasColumnName("largo_m").HasPrecision(5, 2);
        builder.Property(t => t.AnchoMetros).HasColumnName("ancho_m").HasPrecision(5, 2);
        builder.Property(t => t.AlturaMetros).HasColumnName("alto_m").HasPrecision(5, 2);
        builder.Property(t => t.VolumenMaximoMetrosCubicos).HasColumnName("volumen_max_m3").HasPrecision(8, 2);
    }
}

public class VehiculoConfiguration : IEntityTypeConfiguration<Vehiculo>
{
    public void Configure(EntityTypeBuilder<Vehiculo> builder)
    {
        builder.ToTable("vehiculos");
        builder.HasKey(v => v.VehiculoId);
        builder.Property(v => v.VehiculoId).HasColumnName("id");
        builder.Property(v => v.Vin).HasMaxLength(50);
        builder.Property(v => v.Placa).HasMaxLength(20).IsRequired();
        builder.Property(v => v.Propiedad).HasConversion<string>().HasMaxLength(30);
        builder.Property(v => v.Estado).HasConversion<string>().HasMaxLength(30);
        builder.HasIndex(v => new { v.EmpresaId, v.Placa }).IsUnique().HasDatabaseName("ux_vehiculos_empresa_placa");

        builder.HasOne(v => v.TipoVehiculo).WithMany(t => t.Vehiculos)
            .HasForeignKey(v => v.TipoVehiculoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(v => v.Envios).WithOne()
            .HasForeignKey(e => e.VehiculoId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class DocumentoVehiculoConfiguration : IEntityTypeConfiguration<DocumentoVehiculo>
{
    public void Configure(EntityTypeBuilder<DocumentoVehiculo> builder)
    {
        builder.ToTable("documentos_vehiculo");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Tipo).HasConversion<string>().HasMaxLength(50);
        builder.Property(d => d.Numero).HasMaxLength(80).IsRequired();
        builder.HasOne(d => d.Vehiculo).WithMany(v => v.Documentos)
            .HasForeignKey(d => d.VehiculoId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class AsignacionVehiculoConductorConfiguration : IEntityTypeConfiguration<AsignacionVehiculoConductor>
{
    public void Configure(EntityTypeBuilder<AsignacionVehiculoConductor> builder)
    {
        builder.ToTable("asignaciones_vehiculo_conductor");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.FechaInicio).HasColumnName("fecha_asignacion");
        builder.HasOne(a => a.Vehiculo).WithMany(v => v.Asignaciones)
            .HasForeignKey(a => a.VehiculoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.Conductor).WithMany(c => c.Asignaciones)
            .HasForeignKey(a => a.ConductorId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class EnvioConfiguration : IEntityTypeConfiguration<Envio>
{
    public void Configure(EntityTypeBuilder<Envio> builder)
    {
        builder.ToTable("envios");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.TipoServicio).HasConversion<string>().HasMaxLength(30);
        builder.Property(e => e.Estado).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(e => e.PesoKg).HasPrecision(10, 2);
        builder.Property(e => e.VolumenM3).HasPrecision(10, 2);
        builder.Property(e => e.Tarifa).HasPrecision(12, 2);
        builder.Property(e => e.CostoExterno).HasPrecision(12, 2);
    }
}

public class SeguimientoEnvioConfiguration : IEntityTypeConfiguration<SeguimientoEnvio>
{
    public void Configure(EntityTypeBuilder<SeguimientoEnvio> builder)
    {
        builder.ToTable("seguimiento_envio");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Lat).HasPrecision(10, 8);
        builder.Property(s => s.Lng).HasPrecision(11, 8);
        builder.Property(s => s.Estado).HasMaxLength(30).IsRequired();
    }
}
