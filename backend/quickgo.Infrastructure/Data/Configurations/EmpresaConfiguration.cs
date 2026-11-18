using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using quickgo.Domain.entities;

namespace quickgo.Infrastructure.Data.Configurations;

public class EmpresaConfiguration : IEntityTypeConfiguration<Empresa>
{
    public void Configure(EntityTypeBuilder<Empresa> builder)
    {
        builder.ToTable("empresas");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.NombreComercial).HasMaxLength(150).IsRequired();
        builder.Property(e => e.RazonSocial).HasMaxLength(150).IsRequired();
        builder.Property(e => e.Slug).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Rtn).HasMaxLength(20).IsRequired();
        builder.Property(e => e.Tipo).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(e => e.Plan).HasConversion<string>().HasMaxLength(30);
        builder.Property(e => e.Activa).IsRequired();

        builder.HasIndex(e => e.Rtn).IsUnique().HasDatabaseName("ux_empresas_rtn");
        builder.HasIndex(e => e.Slug).IsUnique().HasDatabaseName("ux_empresas_slug");
    }
}
