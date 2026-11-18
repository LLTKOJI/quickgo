using quickgo.Application.Interfaces;

namespace quickgo.API.Security;

/// <summary>Lee la empresa, el usuario y el rol de los claims de la peticion actual.</summary>
public class HttpTenantContext : ITenantContext {
    private readonly IHttpContextAccessor _accessor;

    public HttpTenantContext(IHttpContextAccessor accessor) {
        _accessor = accessor;
    }

    public Guid? EmpresaId => LeerGuid(ClaimNames.EmpresaId);
    public Guid? UserId => LeerGuid(ClaimNames.UserId);
    public string? Rol => Leer(ClaimNames.Rol);
    public string? TipoEmpresa => Leer(ClaimNames.TipoEmpresa);

    private string? Leer(string claim) => _accessor.HttpContext?.User.FindFirst(claim)?.Value;

    private Guid? LeerGuid(string claim) =>
        Guid.TryParse(Leer(claim), out var valor) ? valor : null;
}
