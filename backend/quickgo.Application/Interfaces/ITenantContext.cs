namespace quickgo.Application.Interfaces;

/// <summary>
/// Datos de la peticion actual tomados del token (o de los headers Dev en Development).
/// </summary>
public interface ITenantContext {
    Guid? EmpresaId { get; }
    Guid? UserId { get; }
    string? Rol { get; }
    string? TipoEmpresa { get; }
}
