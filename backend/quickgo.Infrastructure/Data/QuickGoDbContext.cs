using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using quickgo.Application.Interfaces;
using quickgo.Domain.entities;
using quickgo.Domain.Enums;

namespace quickgo.Infrastructure.Data;

public class QuickGoDbContext : DbContext
{
    private readonly ITenantContext _tenant;

    public QuickGoDbContext(DbContextOptions<QuickGoDbContext> options, ITenantContext tenant) : base(options)
    {
        _tenant = tenant;
    }
    
    public DbSet<Empresa> Empresas { get; set; }
    public DbSet<Rol> Roles { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Conductor> Conductores { get; set; }
    public DbSet<Pais> Paises { get; set; }
    public DbSet<Departamento> Departamentos { get; set; }
    public DbSet<Ciudad> Ciudades { get; set; }
    public DbSet<Moneda> Monedas { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Direccion> Direcciones { get; set; }
    public DbSet<Factura> Facturas { get; set; }
    public DbSet<Envio> Envios { get; set; }
    public DbSet<Cotizacion> Cotizaciones { get; set; }
    public DbSet<Pago> Pagos { get; set; }
    public DbSet<Subasta> Subastas { get; set; }
    public DbSet<Puja> Pujas { get; set; }
    public DbSet<TipoVehiculo> TipoVehiculos => Set<TipoVehiculo>();
    public DbSet<Vehiculo> Vehiculos => Set<Vehiculo>();
    public DbSet<DocumentoVehiculo> DocumentosVehiculo => Set<DocumentoVehiculo>();
    public DbSet<AsignacionVehiculoConductor> AsignacionesVehiculoConductor => Set<AsignacionVehiculoConductor>();
    public DbSet<Conductor> Conductores => Set<Conductor>();

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
    }
    
    }
}
