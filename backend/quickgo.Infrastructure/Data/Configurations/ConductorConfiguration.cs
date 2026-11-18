using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using quickgo.Domain.entities;

namespace quickgo.Infrastructure.Data.Configurations;

public class ConductorConfiguration : IEntityTypeConfiguration<Conductor>
{
    public void Configure(EntityTypeBuilder<Conductor> builder)
    {
        builder.ToTable("conductores");

        // PK compartida con usuarios.id (relacion 1 a 1)
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();
        builder.HasOne(c => c.Usuario).WithOne()
            .HasForeignKey<Conductor>(c => c.Id).OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Empresa).WithMany()
            .HasForeignKey(c => c.EmpresaId).OnDelete(DeleteBehavior.Restrict);

        builder.Property(c => c.TipoConductor).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(c => c.Licencia).HasMaxLength(50).IsRequired();
        builder.Property(c => c.VenceLicencia).IsRequired();
        builder.Property(c => c.Estado).HasConversion<string>().HasMaxLength(30).IsRequired();

        // Licencia unica por empresa
        builder.HasIndex(c => new { c.EmpresaId, c.Licencia })
            .IsUnique().HasDatabaseName("ux_conductores_empresa_licencia");
    }
}
