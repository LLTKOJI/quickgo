using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using quickgo.Domain.entities;

namespace quickgo.Infrastructure.Data.Configurations;

public class MonedaConfiguration : IEntityTypeConfiguration<Moneda>
{
    public void Configure(EntityTypeBuilder<Moneda> builder)
    {
        builder.ToTable("monedas");
        // La llave es el codigo de 3 letras (EF no la detecta por convencion)
        builder.HasKey(m => m.Codigo);
        builder.Property(m => m.Codigo).HasMaxLength(3);
        builder.Property(m => m.Nombre).HasMaxLength(50).IsRequired();
        builder.Property(m => m.Simbolo).HasMaxLength(5);
    }
}
