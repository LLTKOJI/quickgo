using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using quickgo.Domain.entities;

namespace quickgo.Infrastructure.Data.Configurations;

public class PaisConfiguration : IEntityTypeConfiguration<Pais>
{
    // GUID fijo: el catalogo de paises es el mismo en todos los ambientes.
    public static readonly Guid HondurasId = new("11111111-0000-0000-0000-000000000001");

    public void Configure(EntityTypeBuilder<Pais> builder)
    {
        // B3-01: semilla inicial. El resto de la configuracion de Pais sigue por convencion.
        builder.HasData(new Pais { Id = HondurasId, Nombre = "Honduras", CodigoIso = "HND" });
    }
}
