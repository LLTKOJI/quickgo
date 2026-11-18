using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using quickgo.Domain.Constants;
using quickgo.Domain.entities;

namespace quickgo.Infrastructure.Data.Configurations;

public class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    // GUID fijos: el catalogo de roles es el mismo en todos los ambientes.
    public static readonly Guid AdminId = new("a0000000-0000-0000-0000-000000000001");
    public static readonly Guid DespachadorId = new("a0000000-0000-0000-0000-000000000002");
    public static readonly Guid ConductorId = new("a0000000-0000-0000-0000-000000000003");
    public static readonly Guid ContratanteId = new("a0000000-0000-0000-0000-000000000004");

    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        builder.ToTable("roles");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Nombre).HasMaxLength(50).IsRequired();
        builder.Property(r => r.Descripcion).HasMaxLength(200);

        builder.HasIndex(r => r.Nombre).IsUnique().HasDatabaseName("ux_roles_nombre");

        builder.HasData(
            new Rol { Id = AdminId, Nombre = Roles.Admin, Descripcion = "Administra la empresa, sus usuarios y la facturación" },
            new Rol { Id = DespachadorId, Nombre = Roles.Despachador, Descripcion = "Gestiona envios, flota y conductores" },
            new Rol { Id = ConductorId, Nombre = Roles.Conductor, Descripcion = "Ejecuta los envios asignados" },
            new Rol { Id = ContratanteId, Nombre = Roles.Contratante, Descripcion = "Publica subastas de transporte" });
    }
}
