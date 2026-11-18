using quickgo.Application.Features.Identidad.DTOs;
using quickgo.Domain.entities;
using quickgo.Domain.enums;

namespace quickgo.Application.Features.Identidad;

public static class Mapeos {
    public static EmpresaResponse ToResponse(this Empresa e) =>
        new(e.Id, e.NombreComercial, e.RazonSocial, e.Slug, e.Rtn, e.Tipo, e.Plan, e.Activa);

    public static UsuarioResponse ToResponse(this Usuario u) =>
        new(u.Id, u.RolId, u.Rol?.Nombre ?? string.Empty, u.NombreCompleto,
            u.PrimerNombre, u.SegundoNombre, u.PrimerApellido, u.SegundoApellido,
            u.Telefono, u.Email, u.Activo);

    public static ConductorResponse ToResponse(this Conductor c) {
        var externo = c.TipoConductor == TipoConductor.Externo;
        var usuario = c.Usuario;
        return new ConductorResponse(
            c.Id,
            usuario?.NombreCompleto ?? string.Empty,
            usuario?.Telefono,
            externo ? null : usuario?.Email,
            c.TipoConductor,
            c.Licencia,
            c.VenceLicencia,
            c.Estado,
            !externo && (usuario?.Activo ?? false));
    }

    public static string? LimpiarOpcional(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
