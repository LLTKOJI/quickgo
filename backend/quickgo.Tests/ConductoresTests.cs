using Microsoft.EntityFrameworkCore;
using quickgo.Application.Common;
using quickgo.Application.Features.Identidad.DTOs;
using quickgo.Domain.Constants;
using quickgo.Domain.enums;

namespace quickgo.Tests;

public class ConductoresTests {
    private readonly Entorno _entorno = new();

    private static CrearConductorRequest Interno(string licencia, string email, DateOnly? vence = null) =>
        new("Juan", null, "Pérez", null, "9999-0000", email, "Secreta123",
            TipoConductor.Interno, licencia, vence ?? DateOnly.FromDateTime(DateTime.UtcNow).AddYears(1));

    private static CrearConductorRequest Externo(string licencia, DateOnly? vence = null) =>
        new("Luis", null, "Mejía", null, null, "ignorado@x.hn", "Ignorada123",
            TipoConductor.Externo, licencia, vence ?? DateOnly.FromDateTime(DateTime.UtcNow).AddYears(1));

    // ---------- B1-04 ----------
    [Fact]
    public async Task Interno_CreaUsuarioConductorYConductorConElMismoId() {
        var s = _entorno.Servicios(Entorno.Tenant(Entorno.EmpresaA, Roles.Despachador));

        var r = await s.Conductores.CrearAsync(Interno("LIC-1", "juan@a.hn"));

        Assert.True(r.Exito);
        Assert.True(r.Valor!.TieneAcceso);
        Assert.Equal("juan@a.hn", r.Valor.Email);
        Assert.Equal(EstadoConductor.Activo, r.Valor.Estado);

        var usuario = await s.Db.Usuarios.SingleAsync(u => u.Id == r.Valor.Id);
        Assert.Equal(Entorno.IdRol(Roles.Conductor), usuario.RolId);
        Assert.True(usuario.Activo);
        Assert.Equal(Entorno.EmpresaA, (await s.Db.Conductores.SingleAsync(c => c.Id == r.Valor.Id)).EmpresaId);
    }

    [Fact]
    public async Task Externo_SeCreaSinAcceso_IgnoraEmailYPassword() {
        var s = _entorno.Servicios(Entorno.Tenant(Entorno.EmpresaA, Roles.Despachador));

        var r = await s.Conductores.CrearAsync(Externo("LIC-EXT"));

        Assert.True(r.Exito);
        Assert.False(r.Valor!.TieneAcceso);
        Assert.Null(r.Valor.Email);

        var usuario = await s.Db.Usuarios.SingleAsync(u => u.Id == r.Valor.Id);
        Assert.False(usuario.Activo);
        Assert.NotEqual("ignorado@x.hn", usuario.Email);

        // y no puede iniciar sesion
        var login = await s.Auth.LoginAsync(new LoginRequest(usuario.Email, "Ignorada123"));
        Assert.False(login.Exito);
    }

    [Fact]
    public async Task Licencia_EsUnicaPorEmpresa_PeroSePuedeRepetirEnOtra() {
        var a = _entorno.Servicios(Entorno.Tenant(Entorno.EmpresaA, Roles.Despachador));
        var b = _entorno.Servicios(Entorno.Tenant(Entorno.EmpresaB, Roles.Despachador));

        await a.Conductores.CrearAsync(Interno("LIC-9", "uno@a.hn"));
        var repetidaEnA = await a.Conductores.CrearAsync(Interno("LIC-9", "dos@a.hn"));
        var igualEnB = await b.Conductores.CrearAsync(Interno("LIC-9", "uno@b.hn"));

        Assert.Equal(TipoError.Conflicto, repetidaEnA.Error);
        Assert.True(igualEnB.Exito);
    }

    [Fact]
    public async Task EmpresaContratante_NoPuedeRegistrarConductores() {
        var c = _entorno.Servicios(
            Entorno.Tenant(Entorno.EmpresaContratante, Roles.Admin, tipo: TipoEmpresa.Contratante));

        var r = await c.Conductores.CrearAsync(Interno("LIC-C", "x@c.hn"));

        Assert.Equal(TipoError.Validacion, r.Error);
    }

    // ---------- B1-05 ----------
    [Fact]
    public async Task ConductorDeOtraEmpresa_Responde404_EnVerYEditar() {
        var b = _entorno.Servicios(Entorno.Tenant(Entorno.EmpresaB, Roles.Despachador));
        var ajeno = (await b.Conductores.CrearAsync(Interno("LIC-B", "c@b.hn"))).Valor!;

        var a = _entorno.Servicios(Entorno.Tenant(Entorno.EmpresaA, Roles.Despachador));
        var ver = await a.Conductores.ObtenerAsync(ajeno.Id);
        var editar = await a.Conductores.ActualizarAsync(ajeno.Id,
            new ActualizarConductorRequest("X", null, "Y", null, null, "NUEVA", ajeno.VenceLicencia, EstadoConductor.Inactivo));
        var lista = await a.Conductores.ListarAsync(new ConductorFiltroQuery());

        Assert.Equal(TipoError.NoEncontrado, ver.Error);
        Assert.Equal(TipoError.NoEncontrado, editar.Error);
        Assert.Empty(lista);
    }

    [Fact]
    public async Task Listar_FiltraPorEstadoYTipo_YEditarCambiaElEstado() {
        var s = _entorno.Servicios(Entorno.Tenant(Entorno.EmpresaA, Roles.Despachador));
        var interno = (await s.Conductores.CrearAsync(Interno("L-1", "i@a.hn"))).Valor!;
        await s.Conductores.CrearAsync(Externo("L-2"));

        var editado = await s.Conductores.ActualizarAsync(interno.Id,
            new ActualizarConductorRequest("Juan", null, "Pérez", null, null, "L-1", interno.VenceLicencia, EstadoConductor.Inactivo));

        Assert.True(editado.Exito);
        Assert.Single(await s.Conductores.ListarAsync(new ConductorFiltroQuery { Estado = EstadoConductor.Inactivo }));
        Assert.Single(await s.Conductores.ListarAsync(new ConductorFiltroQuery { TipoConductor = TipoConductor.Externo }));
        Assert.Equal(2, (await s.Conductores.ListarAsync(new ConductorFiltroQuery())).Count);
    }

    [Fact]
    public async Task RolConductor_SoloVeSuPropioPerfil() {
        var despachador = _entorno.Servicios(Entorno.Tenant(Entorno.EmpresaA, Roles.Despachador));
        var yo = (await despachador.Conductores.CrearAsync(Interno("L-1", "yo@a.hn"))).Valor!;
        var otro = (await despachador.Conductores.CrearAsync(Interno("L-2", "otro@a.hn"))).Valor!;

        var sesion = _entorno.Servicios(Entorno.Tenant(Entorno.EmpresaA, Roles.Conductor, yo.Id));
        var lista = await sesion.Conductores.ListarAsync(new ConductorFiltroQuery());

        Assert.Equal(new[] { yo.Id }, lista.Select(c => c.Id));
        Assert.True((await sesion.Conductores.ObtenerAsync(yo.Id)).Exito);
        Assert.Equal(TipoError.NoEncontrado, (await sesion.Conductores.ObtenerAsync(otro.Id)).Error);
    }

    [Fact]
    public async Task LicenciasPorVencer_OrdenaAscendente_IncluyeVencidasYRespetaElLimite() {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var s = _entorno.Servicios(Entorno.Tenant(Entorno.EmpresaA, Roles.Despachador));
        await s.Conductores.CrearAsync(Interno("EN-20", "a1@a.hn", hoy.AddDays(20)));
        await s.Conductores.CrearAsync(Interno("VENCIDA", "a2@a.hn", hoy.AddDays(-5)));
        await s.Conductores.CrearAsync(Interno("EN-90", "a3@a.hn", hoy.AddDays(90)));
        await s.Conductores.CrearAsync(Interno("EN-10", "a4@a.hn", hoy.AddDays(10)));

        var lista = await s.Conductores.LicenciasPorVencerAsync(30);

        Assert.Equal(new[] { "VENCIDA", "EN-10", "EN-20" }, lista.Select(l => l.Licencia));
        Assert.True(lista[0].Vencida);
        Assert.Equal(-5, lista[0].DiasRestantes);
        Assert.False(lista[1].Vencida);
        Assert.Equal(10, lista[1].DiasRestantes);
    }
}
