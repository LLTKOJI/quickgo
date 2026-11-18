using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using quickgo.Domain.entities;

namespace quickgo.Infrastructure.Data.Configurations;

public class PujaConfiguration : IEntityTypeConfiguration<Puja>
{
    public void Configure(EntityTypeBuilder<Puja> builder)
    {
        builder.ToTable("pujas");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");

        builder.Property(p => p.SubastaId).HasColumnName("subasta_id").IsRequired();
        builder.Property(p => p.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(p => p.ConductorId).HasColumnName("conductor_id").IsRequired();

        builder.Property(p => p.Monto).HasColumnName("monto").HasPrecision(12, 2).IsRequired();
        builder.Property(p => p.Mensaje).HasColumnName("mensaje").HasMaxLength(500);

        builder.Property(p => p.Estado)
            .HasColumnName("estado")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.HasOne(p => p.Subasta)
            .WithMany(s => s.Pujas)
            .HasForeignKey(p => p.SubastaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Empresa)
            .WithMany()
            .HasForeignKey(p => p.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.SubastaId, p.EmpresaId }).IsUnique();
    }
}