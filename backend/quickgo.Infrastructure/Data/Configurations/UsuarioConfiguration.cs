using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using quickgo.Domain.entities;

namespace quickgo.Infrastructure.Data.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.PrimerNombre).HasMaxLength(60).IsRequired();
        builder.Property(u => u.SegundoNombre).HasMaxLength(60);
        builder.Property(u => u.PrimerApellido).HasMaxLength(60).IsRequired();
        builder.Property(u => u.SegundoApellido).HasMaxLength(60);
        builder.Property(u => u.Telefono).HasMaxLength(30);
        builder.Property(u => u.Email).HasMaxLength(150).IsRequired();
        builder.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();
        builder.Property(u => u.Activo).IsRequired();

        builder.HasOne(u => u.Empresa).WithMany()
            .HasForeignKey(u => u.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(u => u.Rol).WithMany()
            .HasForeignKey(u => u.RolId).OnDelete(DeleteBehavior.Restrict);

        // El correo es unico en toda la plataforma (se usa para iniciar sesion).
        builder.HasIndex(u => u.Email).IsUnique().HasDatabaseName("ux_usuarios_email");
        builder.HasIndex(u => u.EmpresaId);
    }
}
