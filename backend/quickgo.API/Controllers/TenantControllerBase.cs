using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace quickgo.API.Controllers;

public abstract class TenantControllerBase : ControllerBase
{
    protected bool HasAnyRole(params string[] roles)
    {
        return User.Identity?.IsAuthenticated == true &&
               roles.Any(User.IsInRole);
    }

    protected bool TryGetEmpresaId(out Guid empresaId)
    {
        empresaId = Guid.Empty;
        if (User.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        var claim = User.FindFirst("EmpresaId")
                    ?? User.FindFirst("empresaId")
                    ?? User.FindFirst("empresa_id");
        return claim is not null &&
               Guid.TryParse(claim.Value, out empresaId) &&
               empresaId != Guid.Empty;
    }

    protected bool TryGetConductorId(out Guid conductorId)
    {
        conductorId = Guid.Empty;
        var claim = User.FindFirst("ConductorId")
                    ?? User.FindFirst("conductorId")
                    ?? User.FindFirst("conductor_id");
        return claim is not null && Guid.TryParse(claim.Value, out conductorId);
    }

    protected bool TryGetUsuarioId(out Guid usuarioId)
    {
        usuarioId = Guid.Empty;
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                    ?? User.FindFirst("sub")
                    ?? User.FindFirst("UsuarioId")
                    ?? User.FindFirst("usuarioId");
        return claim is not null && Guid.TryParse(claim.Value, out usuarioId);
    }

    protected bool IsConductor => User.IsInRole("Conductor");
}
