using quickgo.Domain.enums;

namespace quickgo.Application.Features.Identidad.DTOs;

// ---------- B1-01 Registro e inicio de sesion ----------
public record RegistroEmpresaRequest(
    string NombreComercial,
    string RazonSocial,
    string Rtn,
    TipoEmpresa Tipo,
    PlanEmpresa? Plan,
    string PrimerNombre,
    string? SegundoNombre,
    string PrimerApellido,
    string? SegundoApellido,
    string? Telefono,
    string Email,
    string Password);

public record EmpresaResponse(
    Guid Id,
    string NombreComercial,
    string RazonSocial,
    string Slug,
    string Rtn,
    TipoEmpresa Tipo,
    PlanEmpresa? Plan,
    bool Activa);

public record LoginRequest(string Email, string Password);

public record LoginResponse(
    string Token,
    DateTimeOffset ExpiraEn,
    string NombreCompleto,
    string Rol,
    TipoEmpresa TipoEmpresa);

// ---------- B1-03 Roles y usuarios ----------
public record RolResponse(Guid Id, string Nombre, string? Descripcion);

public record CrearUsuarioRequest(
    string PrimerNombre,
    string? SegundoNombre,
    string PrimerApellido,
    string? SegundoApellido,
    string? Telefono,
    string Email,
    string Password,
    Guid RolId);

public record ActualizarUsuarioRequest(
    string PrimerNombre,
    string? SegundoNombre,
    string PrimerApellido,
    string? SegundoApellido,
    string? Telefono,
    Guid RolId);

public record UsuarioResponse(
    Guid Id,
    Guid RolId,
    string Rol,
    string NombreCompleto,
    string PrimerNombre,
    string? SegundoNombre,
    string PrimerApellido,
    string? SegundoApellido,
    string? Telefono,
    string Email,
    bool Activo);

// ---------- B1-04 / B1-05 Conductores ----------
public record CrearConductorRequest(
    string PrimerNombre,
    string? SegundoNombre,
    string PrimerApellido,
    string? SegundoApellido,
    string? Telefono,
    string? Email,
    string? Password,
    TipoConductor TipoConductor,
    string Licencia,
    DateOnly VenceLicencia);

public record ActualizarConductorRequest(
    string PrimerNombre,
    string? SegundoNombre,
    string PrimerApellido,
    string? SegundoApellido,
    string? Telefono,
    string Licencia,
    DateOnly VenceLicencia,
    EstadoConductor Estado);

public record ConductorFiltroQuery {
    public EstadoConductor? Estado { get; init; }
    public TipoConductor? TipoConductor { get; init; }
}

public record ConductorResponse(
    Guid Id,
    string NombreCompleto,
    string? Telefono,
    string? Email,
    TipoConductor TipoConductor,
    string Licencia,
    DateOnly VenceLicencia,
    EstadoConductor Estado,
    bool TieneAcceso);

public record LicenciasPorVencerQuery {
    public int Dias { get; init; } = 30;
}

public record LicenciaPorVencerResponse(
    Guid ConductorId,
    string NombreCompleto,
    string Licencia,
    DateOnly VenceLicencia,
    int DiasRestantes,
    bool Vencida);
