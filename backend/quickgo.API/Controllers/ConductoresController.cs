using Microsoft.AspNetCore.Mvc;
using quickgo.Application.Interfaces;

namespace quickgo.API.Controllers;

[ApiController]
[Route("api/conductores")]
public class ConductoresController : TenantControllerBase
{
    private readonly IAsignacionRepository _asignacionRepository;

    public ConductoresController(IAsignacionRepository asignacionRepository)
    {
        _asignacionRepository = asignacionRepository;
    }

    [HttpGet("{id:guid}/asignaciones")]
    public async Task<IActionResult> GetAsignaciones(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetEmpresaId(out var empresaId))
        {
            return Unauthorized();
        }

        if (!HasAnyRole("Admin", "Despachador", "Conductor"))
        {
            return Forbid();
        }

        if (IsConductor && !User.IsInRole("Admin") && !User.IsInRole("Despachador"))
        {
            var conductorClaimId = await GetCurrentConductorId(empresaId, cancellationToken);
            if (!conductorClaimId.HasValue || conductorClaimId.Value != id)
            {
                return Forbid();
            }
        }

        var asignaciones = await _asignacionRepository.GetByConductorAsync(
            empresaId,
            id,
            cancellationToken);
        return Ok(asignaciones.Select(AsignacionesController.ToResponse));
    }

    private async Task<Guid?> GetCurrentConductorId(Guid empresaId, CancellationToken cancellationToken)
    {
        if (TryGetConductorId(out var conductorId))
        {
            return conductorId;
        }

        return TryGetUsuarioId(out var usuarioId)
            ? await _asignacionRepository.GetConductorIdByUsuarioAsync(empresaId, usuarioId, cancellationToken)
            : null;
    }
}
