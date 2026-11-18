using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using quickgo.Application.Interfaces;
using quickgo.Domain.entities;

namespace quickgo.Infrastructure.Data;

public class QuickGoDbContext : DbContext
{
    private readonly ITenantContext _tenant;

    public QuickGoDbContext(DbContextOptions<QuickGoDbContext> options, ITenantContext tenant) : base(options)
    {
        _tenant = tenant;
    }

    public DbSet<TipoVehiculo> TiposVehiculo { get; set; }
    public DbSet<Vehiculo> Vehiculos { get; set; }
    public DbSet<Empresa> Empresas { get; set; }
    public DbSet<Rol> Roles { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Conductor> Conductores { get; set; }
    public DbSet<Pais> Paises { get; set; }
    public DbSet<Departamento> Departamentos { get; set; }
    public DbSet<Ciudad> Ciudades { get; set; }
    public DbSet<Moneda> Monedas { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Subasta> Subastas { get; set; }
    public DbSet<Puja> Pujas { get; set; }

    // Lo leen los filtros globales: se evalua en cada consulta con la empresa del token.
    // Sin empresa (null) no devuelve nada (falla cerrado).
    private Guid? EmpresaActualId => _tenant.EmpresaId;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1) Nombres snake_case por defecto (tablas y columnas), como en el ERD.
        //    Las configuraciones explicitas del paso 2 tienen prioridad.
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            var tabla = entity.GetTableName();
            if (tabla is not null)
            {
                entity.SetTableName(ASnake(tabla));
            }
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(ASnake(property.Name));
            }
        }

        // 2) Configuraciones por entidad (IEntityTypeConfiguration)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(QuickGoDbContext).Assembly);

        // 3) Filtro global por empresa_id para toda entidad IEmpresaScoped
        var metodo = typeof(QuickGoDbContext).GetMethod(nameof(AplicarFiltroEmpresa),
            BindingFlags.NonPublic | BindingFlags.Instance)!;
        foreach (var entity in modelBuilder.Model.GetEntityTypes()
                     .Where(t => typeof(IEmpresaScoped).IsAssignableFrom(t.ClrType)))
        {
            metodo.MakeGenericMethod(entity.ClrType).Invoke(this, new object[] { modelBuilder });
        }
    }

    private void AplicarFiltroEmpresa<T>(ModelBuilder modelBuilder) where T : class, IEmpresaScoped
    {
        modelBuilder.Entity<T>().HasQueryFilter(e => e.EmpresaId == EmpresaActualId);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AsignarEmpresaEnInsertados();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        AsignarEmpresaEnInsertados();
        return base.SaveChanges();
    }

    // Al insertar, la empresa sale del token. Solo se completa cuando la entidad no trae empresa
    // (nunca viene del cuerpo de la peticion); asi el registro de empresas y la adjudicacion de
    // subastas, que escriben con una empresa explicita, no se pisan.
    private void AsignarEmpresaEnInsertados()
    {
        if (_tenant.EmpresaId is not Guid empresaId)
        {
            return;
        }

        foreach (var entry in ChangeTracker.Entries<IEmpresaScoped>()
                     .Where(e => e.State == EntityState.Added && e.Entity.EmpresaId == Guid.Empty))
        {
            entry.Entity.EmpresaId = empresaId;
        }
    }

    private static string ASnake(string nombre) =>
        Regex.Replace(nombre, "([a-z0-9])([A-Z])", "$1_$2").ToLowerInvariant();
}
