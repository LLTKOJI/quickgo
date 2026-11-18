using Microsoft.EntityFrameworkCore;
using quickgo.Application.Interfaces;
using quickgo.Application.Services;
using quickgo.Domain.Constants;
using quickgo.Domain.entities;
using quickgo.Domain.enums;
using quickgo.Infrastructure.Data;
using quickgo.Infrastructure.Data.Configurations;
using quickgo.Infrastructure.Repositories;
using quickgo.Infrastructure.Security;

namespace quickgo.Tests;

public class TenantFalso : ITenantContext {
    public Guid? EmpresaId { get; set; }
    public Guid? UserId { get; set; }
    public string? Rol { get; set; }
    public string? TipoEmpresa { get; set; }
}

public class TokenFalso : ITokenService {
    public TokenGenerado Generar(Guid userId, Guid empresaId, string rol, TipoEmpresa tipoEmpresa) =>
        new("token-de-prueba", DateTimeOffset.UtcNow.AddHours(1));
}

/// <summary>Una "base" en memoria compartida; cada contexto simula la peticion de una empresa distinta.</summary>
public class Entorno {
    public static readonly Guid EmpresaA = Guid.Parse("aaaaaaaa-0000-0000-0000-00000000000a");
    public static readonly Guid EmpresaB = Guid.Parse("bbbbbbbb-0000-0000-0000-00000000000b");
    public static readonly Guid EmpresaContratante = Guid.Parse("cccccccc-0000-0000-0000-00000000000c");

    private readonly string _nombreBase = Guid.NewGuid().ToString();
    private readonly Pbkdf2PasswordHasher _hasher = new();

    public Entorno() {
        // Sin empresa en el contexto: se siembran las empresas sin filtro.
        using var db = CrearDb(new TenantFalso());
        db.Database.EnsureCreated();
        db.Empresas.AddRange(
            Nueva(EmpresaA, "Transportes A", "RTN-A", TipoEmpresa.Transportista),
            Nueva(EmpresaB, "Transportes B", "RTN-B", TipoEmpresa.Transportista),
            Nueva(EmpresaContratante, "Contratante C", "RTN-C", TipoEmpresa.Contratante));
        db.SaveChanges();
    }

    private static Empresa Nueva(Guid id, string nombre, string rtn, TipoEmpresa tipo) => new() {
        Id = id, NombreComercial = nombre, RazonSocial = nombre, Slug = nombre.ToLowerInvariant().Replace(' ', '-'),
        Rtn = rtn, Tipo = tipo, Plan = PlanEmpresa.Basico, Activa = true
    };

    public QuickGoDbContext CrearDb(ITenantContext tenant) {
        var options = new DbContextOptionsBuilder<QuickGoDbContext>()
            .UseInMemoryDatabase(_nombreBase)
            .Options;
        return new QuickGoDbContext(options, tenant);
    }

    public static TenantFalso Tenant(Guid empresaId, string rol, Guid? userId = null,
        TipoEmpresa tipo = TipoEmpresa.Transportista) =>
        new() { EmpresaId = empresaId, Rol = rol, UserId = userId ?? Guid.NewGuid(), TipoEmpresa = tipo.ToString() };

    public ServiciosPrueba Servicios(ITenantContext tenant) => new(this, tenant);

    public Usuario NuevoUsuario(Guid empresaId, string rol, string email, bool activo = true) => new() {
        Id = Guid.NewGuid(),
        EmpresaId = empresaId,
        RolId = IdRol(rol),
        PrimerNombre = "Nombre",
        PrimerApellido = "Apellido",
        Email = email,
        PasswordHash = _hasher.Hash("Secreta123"),
        Activo = activo
    };

    public static Guid IdRol(string rol) => rol switch {
        Roles.Admin => RolConfiguration.AdminId,
        Roles.Despachador => RolConfiguration.DespachadorId,
        Roles.Conductor => RolConfiguration.ConductorId,
        _ => RolConfiguration.ContratanteId
    };
}

public class ServiciosPrueba {
    public QuickGoDbContext Db { get; }
    public EmpresaService Empresas { get; }
    public AuthService Auth { get; }
    public UsuarioService Usuarios { get; }
    public ConductorService Conductores { get; }

    public ServiciosPrueba(Entorno entorno, ITenantContext tenant) {
        Db = entorno.CrearDb(tenant);
        var hasher = new Pbkdf2PasswordHasher();
        var uow = new UnitOfWork(Db);
        var usuarios = new UsuarioRepository(Db);
        var roles = new RolRepository(Db);
        var empresas = new EmpresaRepository(Db);
        var conductores = new ConductorRepository(Db);

        Empresas = new EmpresaService(empresas, usuarios, roles, hasher, uow);
        Auth = new AuthService(usuarios, hasher, new TokenFalso());
        Usuarios = new UsuarioService(usuarios, roles, empresas, hasher, uow, tenant);
        Conductores = new ConductorService(conductores, usuarios, roles, empresas, hasher, uow, tenant);
    }
}
