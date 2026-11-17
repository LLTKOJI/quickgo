using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using quickgo.Domain.entities;

namespace quickgo.Infrastructure.Data.Configurations;

public class SubastaConfiguration : IEntityTypeConfiguration<Subasta>
{
    public void Configure(EntityTypeBuilder<Subasta> builder)
    {
        builder.ToTable("subastas");

        builder.HasKey(s => s.SubastaId);
        builder.Property(s => s.SubastaId).HasColumnName("id");

        builder.Property(s => s.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(s => s.TipoVehiculoId).HasColumnName("tipo_vehiculo_id").IsRequired();
        builder.Property(s => s.OrigenDireccionId).HasColumnName("origen_direccion_id").IsRequired();
        builder.Property(s => s.DestinoDireccionId).HasColumnName("destino_direccion_id").IsRequired();

        builder.Property(s => s.Descripcion).HasColumnName("descripcion").HasMaxLength(500);
        builder.Property(s => s.PesoKg).HasColumnName("peso_kg").HasPrecision(10, 2).IsRequired();
        builder.Property(s => s.VolumenM3).HasColumnName("volumen_m3").HasPrecision(10, 2);
        builder.Property(s => s.PrecioMaximo).HasColumnName("precio_maximo").HasPrecision(12, 2).IsRequired();
        builder.Property(s => s.MonedaCodigo).HasColumnName("moneda_codigo").HasMaxLength(3).IsRequired();
        builder.Property(s => s.FechaCierre).HasColumnName("fecha_cierre").IsRequired();

        builder.Property(s => s.Estado)
            .HasColumnName("estado")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.HasOne(s => s.Empresa)
            .WithMany()
            .HasForeignKey(s => s.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.TipoVehiculo)
            .WithMany()
            .HasForeignKey(s => s.TipoVehiculoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices para las consultas más frecuentes
        builder.HasIndex(s => s.EmpresaId);
        builder.HasIndex(s => new { s.Estado, s.FechaCierre });
    }
}