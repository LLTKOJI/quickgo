using Microsoft.EntityFrameworkCore;
using quickgo.Domain.entities;

namespace quickgo.Infrastructure.Data;

public class QuickGoDbContext: DbContext
{
    public QuickGoDbContext(DbContextOptions<QuickGoDbContext> options) : base(options)
    {
        
    }
    public DbSet<TipoVehiculo>TipoVehiculos { get; set; }
    public DbSet<Vehiculo>Vehiculos { get; set; }
    public DbSet<Empresa>Empresas  { get; set; }
    public DbSet<Subasta> Subastas { get; set; }
    public DbSet<Puja> Pujas { get; set; }

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(QuickGoDbContext).Assembly);
}
}