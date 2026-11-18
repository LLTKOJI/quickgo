using Microsoft.EntityFrameworkCore;
using quickgo.Application.Common;
using quickgo.Application.Features.Identidad.DTOs;
using quickgo.Domain.Constants;
using quickgo.Domain.enums;

namespace quickgo.Tests;

public class IdentidadTests {
    private readonly Entorno _entorno = new();

    private static RegistroEmpresaRequest Registro(string nombre = "Mi Empresa", string rtn = "0801-1",
        string email = "admin@mi.hn", TipoEmpresa tipo = TipoEmpresa.Transportista) =>
        new(nombre, nombre + " S.A.", rtn, tipo, null, "Ana", null, "López", null, null, email, "Secreta123");

    // ---------- B1-01 ----------
    [Fact]
    public async Task Registro_CreaEmpresaYAdmin_ConSlugYPlanBasico() {
        var s = _entorno.Servicios(new TenantFalso());

        var r = await s.Empresas.RegistrarAsync(Registro("Transportes Ñandú Ltda."));

        Assert.True(r.Exito);
        Assert.Equal("transportes-nandu-ltda", r.Valor!.Slug);
        Assert.Equal(PlanEmpresa.Basico, r.Valor.Plan);

        var admin = await s.Db.Usuarios.IgnoreQueryFilters().SingleAsync(u => u.EmpresaId == r.Valor.Id);
        Assert.Equal(RolDeAdmin(), admin.RolId);
        Assert.NotEqual("Secreta123", admin.PasswordHash); // nunca en texto plano
    }

    [Fact]
    public async Task Registro_RtnOEmailDuplicado_Responde409() {
        var s = _entorno.Servicios(new TenantFalso());
        await s.Empresas.RegistrarAsync(Registro());

        var rtnRepetido = await s.Empresas.RegistrarAsync(Registro("Otra", "0801-1", "otro@mi.hn"));
        var emailRepetido = await s.Empresas.RegistrarAsync(Registro("Otra", "0801-2", "admin@mi.hn"));

        Assert.Equal(TipoError.Conflicto, rtnRepetido.Error);
        Assert.Equal(TipoError.Conflicto, emailRepetido.Error);
    }

    [Fact]
    public async Task Login_CredencialesInvalidas_MismoMensaje() {
        var s = _entorno.Servicios(new TenantFalso());
        await s.Empresas.RegistrarAsync(Registro());

        var correoInexistente = await s.Auth.LoginAsync(new LoginRequest("nadie@x.hn", "Secreta123"));
        var passwordMala = await s.Auth.LoginAsync(new LoginRequest("admin@mi.hn", "OtraClave999"));

        Assert.Equal(TipoError.NoAutenticado, correoInexistente.Error);
        Assert.Equal(TipoError.NoAutenticado, passwordMala.Error);
        Assert.Equal(correoInexistente.Detalle, passwordMala.Detalle);
    }

    [Fact]
    public async Task Login_Correcto_DevuelveTokenRolYTipoEmpresa() {
        var s = _entorno.Servicios(new TenantFalso());
        await s.Empresas.RegistrarAsync(Registro(tipo: TipoEmpresa.Contratante));

        var r = await s.Auth.LoginAsync(new LoginRequest("ADMIN@mi.hn", "Secreta123"));

        Assert.True(r.Exito);
        Assert.Equal(Roles.Admin, r.Valor!.Rol);
        Assert.Equal(TipoEmpresa.Contratante, r.Valor.TipoEmpresa);
        Assert.False(string.IsNullOrEmpty(r.Valor.Token));
    }

    [Fact]
    public async Task Login_UsuarioInactivo_Responde403() {
        var s = _entorno.Servicios(new TenantFalso());
        s.Db.Usuarios.Add(_entorno.NuevoUsuario(Entorno.EmpresaA, Roles.Despachador, "baja@a.hn", activo: false));
        await s.Db.SaveChangesAsync();

        var r = await s.Auth.LoginAsync(new LoginRequest("baja@a.hn", "Secreta123"));

        Assert.Equal(TipoError.Prohibido, r.Error);
    }

    // ---------- B1-02 ----------
    [Fact]
    public async Task FiltroGlobal_UnaEmpresaNoVeUsuariosDeOtra() {
        var a = _entorno.Servicios(Entorno.Tenant(Entorno.EmpresaA, Roles.Admin));
        a.Db.Usuarios.Add(_entorno.NuevoUsuario(Entorno.EmpresaA, Roles.Admin, "a@a.hn"));
        a.Db.Usuarios.Add(_entorno.NuevoUsuario(Entorno.EmpresaB, Roles.Admin, "b@b.hn"));
        await a.Db.SaveChangesAsync();

        var vistosPorA = await a.Usuarios.ListarAsync();
        var b = _entorno.Servicios(Entorno.Tenant(Entorno.EmpresaB, Roles.Admin));
        var vistosPorB = await b.Usuarios.ListarAsync();

        Assert.Equal(new[] { "a@a.hn" }, vistosPorA.Select(u => u.Email));
        Assert.Equal(new[] { "b@b.hn" }, vistosPorB.Select(u => u.Email));
    }

    [Fact]
    public async Task Insertar_AsignaLaEmpresaDelToken_CuandoNoVieneEmpresa() {
        var a = _entorno.Servicios(Entorno.Tenant(Entorno.EmpresaA, Roles.Admin));
        var usuario = _entorno.NuevoUsuario(Guid.Empty, Roles.Despachador, "sin-empresa@a.hn");
        a.Db.Usuarios.Add(usuario);
        await a.Db.SaveChangesAsync();

        Assert.Equal(Entorno.EmpresaA, usuario.EmpresaId);
    }

    [Fact]
    public async Task SinEmpresaEnElToken_NoSeVeNada() {
        var conEmpresa = _entorno.Servicios(Entorno.Tenant(Entorno.EmpresaA, Roles.Admin));
        conEmpresa.Db.Usuarios.Add(_entorno.NuevoUsuario(Entorno.EmpresaA, Roles.Admin, "a@a.hn"));
        await conEmpresa.Db.SaveChangesAsync();

        var sinEmpresa = _entorno.Servicios(new TenantFalso());

        Assert.Empty(await sinEmpresa.Db.Usuarios.ToListAsync());
    }

    // ---------- B1-03 ----------
    [Fact]
    public async Task CrearUsuario_RolIncompatibleConElTipoDeEmpresa_Responde400() {
        var contratante = _entorno.Servicios(
            Entorno.Tenant(Entorno.EmpresaContratante, Roles.Admin, tipo: TipoEmpresa.Contratante));
        var transportista = _entorno.Servicios(Entorno.Tenant(Entorno.EmpresaA, Roles.Admin));

        var despachadorEnContratante = await contratante.Usuarios.CrearAsync(Crear("d@c.hn", Roles.Despachador));
        var contratanteEnTransportista = await transportista.Usuarios.CrearAsync(Crear("c@a.hn", Roles.Contratante));
        var despachadorEnTransportista = await transportista.Usuarios.CrearAsync(Crear("d@a.hn", Roles.Despachador));

        Assert.Equal(TipoError.Validacion, despachadorEnContratante.Error);
        Assert.Equal(TipoError.Validacion, contratanteEnTransportista.Error);
        Assert.True(despachadorEnTransportista.Exito);
        Assert.Equal(Entorno.EmpresaA, (await transportista.Db.Usuarios.SingleAsync(u => u.Email == "d@a.hn")).EmpresaId);
    }

    [Fact]
    public async Task CrearUsuario_EmailDuplicadoEnOtraEmpresa_Responde409() {
        var b = _entorno.Servicios(Entorno.Tenant(Entorno.EmpresaB, Roles.Admin));
        b.Db.Usuarios.Add(_entorno.NuevoUsuario(Entorno.EmpresaB, Roles.Admin, "repetido@x.hn"));
        await b.Db.SaveChangesAsync();

        var a = _entorno.Servicios(Entorno.Tenant(Entorno.EmpresaA, Roles.Admin));
        var r = await a.Usuarios.CrearAsync(Crear("repetido@x.hn", Roles.Despachador));

        Assert.Equal(TipoError.Conflicto, r.Error);
    }

    [Fact]
    public async Task UsuarioDeOtraEmpresa_EditarYDesactivar_Responden404() {
        var b = _entorno.Servicios(Entorno.Tenant(Entorno.EmpresaB, Roles.Admin));
        var ajeno = _entorno.NuevoUsuario(Entorno.EmpresaB, Roles.Despachador, "ajeno@b.hn");
        b.Db.Usuarios.Add(ajeno);
        await b.Db.SaveChangesAsync();

        var a = _entorno.Servicios(Entorno.Tenant(Entorno.EmpresaA, Roles.Admin));
        var editar = await a.Usuarios.ActualizarAsync(ajeno.Id,
            new ActualizarUsuarioRequest("X", null, "Y", null, null, Entorno.IdRol(Roles.Despachador)));
        var desactivar = await a.Usuarios.DesactivarAsync(ajeno.Id);

        Assert.Equal(TipoError.NoEncontrado, editar.Error);
        Assert.Equal(TipoError.NoEncontrado, desactivar.Error);
        Assert.True((await b.Db.Usuarios.SingleAsync(u => u.Id == ajeno.Id)).Activo);
    }

    [Fact]
    public async Task Desactivar_NoPermiteASiMismoNiAlUltimoAdmin_YHaceBajaLogicaDelResto() {
        var admin = _entorno.NuevoUsuario(Entorno.EmpresaA, Roles.Admin, "admin@a.hn");
        var despachador = _entorno.NuevoUsuario(Entorno.EmpresaA, Roles.Despachador, "desp@a.hn");
        var seed = _entorno.Servicios(Entorno.Tenant(Entorno.EmpresaA, Roles.Admin));
        seed.Db.Usuarios.AddRange(admin, despachador);
        await seed.Db.SaveChangesAsync();

        // Otro admin distinto (que no es el unico) intenta desactivar al unico Admin activo
        var otroAdminSesion = _entorno.Servicios(Entorno.Tenant(Entorno.EmpresaA, Roles.Admin, Guid.NewGuid()));
        var ultimoAdmin = await otroAdminSesion.Usuarios.DesactivarAsync(admin.Id);
        Assert.Equal(TipoError.Conflicto, ultimoAdmin.Error);

        // El admin intenta desactivarse a si mismo
        var sesionDelAdmin = _entorno.Servicios(Entorno.Tenant(Entorno.EmpresaA, Roles.Admin, admin.Id));
        var asiMismo = await sesionDelAdmin.Usuarios.DesactivarAsync(admin.Id);
        Assert.Equal(TipoError.Conflicto, asiMismo.Error);

        // Un usuario normal si se puede dar de baja, y la fila sigue existiendo
        var ok = await sesionDelAdmin.Usuarios.DesactivarAsync(despachador.Id);
        Assert.True(ok.Exito);
        Assert.False(ok.Valor!.Activo);
        Assert.Equal(2, await sesionDelAdmin.Db.Usuarios.CountAsync());
    }

    [Fact]
    public async Task Roles_DevuelveLosCuatroDelCatalogo() {
        var s = _entorno.Servicios(Entorno.Tenant(Entorno.EmpresaA, Roles.Admin));
        var roles = await new quickgo.Application.Services.RolService(
            new quickgo.Infrastructure.Repositories.RolRepository(s.Db)).ListarAsync();

        Assert.Equal(new[] { "Admin", "Conductor", "Contratante", "Despachador" }, roles.Select(r => r.Nombre));
    }

    private static CrearUsuarioRequest Crear(string email, string rol) =>
        new("Nuevo", null, "Usuario", null, null, email, "Secreta123", Entorno.IdRol(rol));

    private static Guid RolDeAdmin() => Entorno.IdRol(Roles.Admin);
}
