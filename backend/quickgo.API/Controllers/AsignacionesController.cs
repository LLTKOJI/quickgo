using Microsoft.AspNetCore.Mvc;
using quickgo.Application.Features.Asignacion;
using quickgo.Application.Features.Asignacion.DTOs;
using quickgo.Application.Interfaces;
using quickgo.Domain.entities;

namespace quickgo.API.Controllers;

[ApiController]
[Route("api/asignaciones")]
public class AsignacionesController : TenantControllerBase
{
    private readonly IAsignacionRepository _asignacionRepository;

    public AsignacionesController(IAsignacionRepository asignacionRepository)
    {
        _asignacionRepository = asignacionRepository;
    }

    [HttpPost]
    [EndpointSummary("Crear asignacion vehiculo-conductor")]
    [EndpointDescription("Roles: Admin, Despachador. Crea una asignacion activa entre vehiculo y conductor de la misma empresa. Valida disponibilidad y estado.")]
    [ProducesResponseType(typeof(AsignacionResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        CrearAsignacionRequestDTO request,
        CancellationToken cancellationToken)
    {
        if (!TryGetEmpresaId(out var empresaId))
        {
            return Unauthorized();
        }

        if (!HasAnyRole("Admin", "Despachador"))
        {
            return Forbid();
        }

        var result = await _asignacionRepository.CrearAsync(
            empresaId,
            request.VehiculoId,
            request.ConductorId,
            cancellationToken);

        return result switch
        {
            CrearAsignacionResult.Success => await GetCreatedResponse(empresaId, request.VehiculoId, cancellationToken),
            CrearAsignacionResult.VehicleNotFound => NotFound("El vehículo no pertenece a esta empresa."),
            CrearAsignacionResult.VehicleUnavailable => Conflict("No se puede asignar un vehículo dado de baja."),
            CrearAsignacionResult.ConductorNotFound => NotFound("El conductor no pertenece a esta empresa."),
            CrearAsignacionResult.ConductorInactive => Conflict("El conductor o su usuario están inactivos."),
            _ => Problem("No se pudo crear la asignación.")
        };
    }

    private async Task<IActionResult> GetCreatedResponse(
        Guid empresaId,
        Guid vehiculoId,
        CancellationToken cancellationToken)
    {
        var asignaciones = await _asignacionRepository.GetByVehiculoAsync(
            empresaId,
            vehiculoId,
            cancellationToken: cancellationToken);
        var asignacion = asignaciones.FirstOrDefault(item => item.FechaFin is null);
        return asignacion is null
            ? Problem("La asignación se creó, pero no se pudo volver a consultar.")
            : Created($"/api/vehiculos/{vehiculoId}/asignaciones", ToResponse(asignacion));
    }

    [HttpPatch("{id:guid}/finalizar")]
    [EndpointSummary("Finalizar asignacion")]
    [EndpointDescription("Roles: Admin, Despachador, Conductor. Finaliza una asignacion activa. El conductor solo puede finalizar las suyas.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Finalizar(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetEmpresaId(out var empresaId))
        {
            return Unauthorized();
        }

        if (!HasAnyRole("Admin", "Despachador", "Conductor"))
        {
            return Forbid();
        }

        Guid? conductorId = null;
        if (IsConductor && !User.IsInRole("Admin") && !User.IsInRole("Despachador"))
        {
            if (TryGetConductorId(out var claimConductorId))
            {
                conductorId = claimConductorId;
            }
            else if (TryGetUsuarioId(out var usuarioId))
            {
                conductorId = await _asignacionRepository.GetConductorIdByUsuarioAsync(
                    empresaId,
                    usuarioId,
                    cancellationToken);
            }

            if (!conductorId.HasValue)
            {
                return Forbid();
            }
        }

        var result = await _asignacionRepository.FinalizarAsync(
            empresaId,
            id,
            conductorId,
            cancellationToken);
        return result switch
        {
            FinalizarAsignacionResult.Success => NoContent(),
            FinalizarAsignacionResult.NotFound => NotFound(),
            FinalizarAsignacionResult.AlreadyFinished => Conflict("La asignación ya está finalizada."),
            _ => Problem("No se pudo finalizar la asignación.")
        };
    }

    internal static AsignacionResponseDTO ToResponse(AsignacionVehiculoConductor asignacion)
    {
        return new AsignacionResponseDTO
        {
            Id = asignacion.Id,
            VehiculoId = asignacion.VehiculoId,
            Placa = asignacion.Vehiculo.Placa,
            ConductorId = asignacion.ConductorId,
            NombreConductor = asignacion.Conductor.Usuario?.NombreCompleto ?? string.Empty,
            FechaInicio = asignacion.FechaInicio,
            FechaFin = asignacion.FechaFin,
            Vigente = asignacion.FechaFin == null
        };
    }
}
