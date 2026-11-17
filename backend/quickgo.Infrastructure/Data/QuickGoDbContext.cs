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
}