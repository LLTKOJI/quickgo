using Microsoft.AspNetCore.Mvc;
using quickgo.Application.Features.Asignacion.DTOs;
using quickgo.Application.Features.Vehiculo;
using quickgo.Application.Features.Vehiculo.DTOs;
using quickgo.Application.Interfaces;
using quickgo.Domain.entities;
using quickgo.Domain.Enums;

namespace quickgo.API.Controllers;

[ApiController]
[Route("api/vehiculos")]
public class VehiculoController : TenantControllerBase
{
    private readonly IVehiculoRepository _vehiculoRepository;
    private readonly IDocumentoVehiculoRepository _documentoRepository;
    private readonly IAsignacionRepository _asignacionRepository;

    public VehiculoController(
        IVehiculoRepository vehiculoRepository,
        IDocumentoVehiculoRepository documentoRepository,
        IAsignacionRepository asignacionRepository)
    {
        _vehiculoRepository = vehiculoRepository;
        _documentoRepository = documentoRepository;
        _asignacionRepository = asignacionRepository;
    }

    [HttpGet]
    [EndpointSummary("Listar vehiculos")]
    [EndpointDescription("Roles: Admin, Despachador, Conductor. Lista vehiculos de la empresa. Filtros opcionales por estado, tipoVehiculoId e incluirBaja.")]
    [ProducesResponseType(typeof(IEnumerable<VehiculoResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetVehiculos(
        [FromQuery] VehiculoFiltroQuery filtro,
        CancellationToken cancellationToken)
    {
        if (!TryGetEmpresaId(out var empresaId))
        {
            return Unauthorized();
        }

        if (!HasAnyRole("Admin", "Despachador", "Conductor"))
        {
            return Forbid();
        }

        if (filtro.Estado.HasValue && !Enum.IsDefined(filtro.Estado.Value))
        {
            return BadRequest("El estado indicado no es válido.");
        }

        var vehiculos = await _vehiculoRepository.GetAllVehiculosAsync(
            empresaId,
            filtro.Estado,
            filtro.TipoVehiculoId,
            filtro.IncluirBaja,
            cancellationToken);

        return Ok(vehiculos.Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Obtener vehiculo por id")]
    [EndpointDescription("Roles: Admin, Despachador, Conductor. Devuelve el vehiculo de la empresa. Si no existe o es de otra empresa responde 404.")]
    [ProducesResponseType(typeof(VehiculoResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVehiculo(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetEmpresaId(out var empresaId))
        {
            return Unauthorized();
        }

        if (!HasAnyRole("Admin", "Despachador", "Conductor"))
        {
            return Forbid();
        }

        var vehiculo = await _vehiculoRepository.GetVehiculoByIdAsync(empresaId, id, cancellationToken);
        return vehiculo is null ? NotFound() : Ok(ToResponse(vehiculo));
    }

    [HttpPost]
    [EndpointSummary("Crear vehiculo")]
    [EndpointDescription("Roles: Admin, Despachador. Registra un vehiculo en la empresa. Valida tipo de vehiculo y placa unica por empresa.")]
    [ProducesResponseType(typeof(VehiculoResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateVehiculo(
        CreateVehiculoRequestDTO request,
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

        if (!Enum.IsDefined(request.Propiedad))
        {
            return BadRequest("La propiedad indicada no es válida.");
        }

        if (!await _vehiculoRepository.TipoVehiculoExistsAsync(request.TipoVehiculoId, cancellationToken))
        {
            return BadRequest("El tipo de vehículo no existe.");
        }

        var placa = request.Placa.Trim().ToUpperInvariant();
        if (await _vehiculoRepository.PlacaExistsAsync(empresaId, placa, cancellationToken: cancellationToken))
        {
            return Conflict("La placa ya está registrada para esta empresa.");
        }

        var vehiculo = await _vehiculoRepository.AddVehiculoAsync(
            new Vehiculo
            {
                VehiculoId = Guid.NewGuid(),
                EmpresaId = empresaId,
                TipoVehiculoId = request.TipoVehiculoId,
                Placa = placa,
                Vin = string.IsNullOrWhiteSpace(request.Vin) ? null : request.Vin.Trim(),
                Propiedad = request.Propiedad,
                Estado = EstadoVehiculo.Disponible
            },
            cancellationToken);

        var response = await _vehiculoRepository.GetVehiculoByIdAsync(
            empresaId, vehiculo.VehiculoId, cancellationToken);
        if (response is null)
        {
            return Problem("El vehículo fue creado, pero no se pudo volver a consultar.");
        }

        return CreatedAtAction(nameof(GetVehiculo), new { id = vehiculo.VehiculoId }, ToResponse(response));
    }

    [HttpPut("{id:guid}")]
    [EndpointSummary("Actualizar vehiculo")]
    [EndpointDescription("Roles: Admin, Despachador. Actualiza datos del vehiculo. No permite cambiar a estado Baja por este endpoint.")]
    [ProducesResponseType(typeof(VehiculoResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateVehiculo(
        Guid id,
        ActualizarVehiculoRequestDTO request,
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

        if (!Enum.IsDefined(request.Estado) || request.Estado == EstadoVehiculo.Baja)
        {
            return BadRequest("El estado no es válido. Use la operación de baja para retirar un vehículo.");
        }

        var placa = string.IsNullOrWhiteSpace(request.Placa)
            ? null
            : request.Placa.Trim().ToUpperInvariant();
        if (placa is not null &&
            await _vehiculoRepository.PlacaExistsAsync(empresaId, placa, id, cancellationToken))
        {
            return Conflict("La placa ya está registrada para esta empresa.");
        }

        var vehiculo = await _vehiculoRepository.UpdateVehiculoAsync(
            empresaId,
            id,
            request.Estado,
            request.Vin?.Trim(),
            placa,
            cancellationToken);
        if (vehiculo is null)
        {
            return NotFound();
        }

        var response = await _vehiculoRepository.GetVehiculoByIdAsync(empresaId, id, cancellationToken);
        return response is null
            ? Problem("El vehículo se actualizó, pero no se pudo volver a consultar.")
            : Ok(ToResponse(response));
    }

    [HttpPatch("{id:guid}/baja")]
    [EndpointSummary("Dar de baja vehiculo")]
    [EndpointDescription("Roles: Admin. Da de baja logica al vehiculo. No permite baja si tiene envios asignados o en curso.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DarDeBaja(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetEmpresaId(out var empresaId))
        {
            return Unauthorized();
        }

        if (!HasAnyRole("Admin"))
        {
            return Forbid();
        }

        var result = await _vehiculoRepository.DarDeBajaAsync(empresaId, id, cancellationToken);
        return result switch
        {
            BajaVehiculoResult.Success => NoContent(),
            BajaVehiculoResult.NotFound => NotFound(),
            BajaVehiculoResult.HasActiveShipment =>
                Conflict("No se puede dar de baja un vehículo con envíos asignados o en curso."),
            _ => Problem("No se pudo completar la baja del vehículo.")
        };
    }

    [HttpPost("{id:guid}/documentos")]
    [EndpointSummary("Agregar documento al vehiculo")]
    [EndpointDescription("Roles: Admin. Registra documento del vehiculo (por ejemplo Seguro). La fecha de vencimiento es obligatoria.")]
    [ProducesResponseType(typeof(DocumentoResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateDocumento(
        Guid id,
        CrearDocumentoRequestDTO request,
        CancellationToken cancellationToken)
    {
        if (!TryGetEmpresaId(out var empresaId))
        {
            return Unauthorized();
        }

        if (!HasAnyRole("Admin"))
        {
            return Forbid();
        }

        if (!Enum.IsDefined(request.Tipo) || request.FechaVencimiento == default)
        {
            return BadRequest("El tipo y la fecha de vencimiento del documento son obligatorios y deben ser válidos.");
        }

        var documento = await _documentoRepository.AddAsync(
            empresaId,
            id,
            new DocumentoVehiculo
            {
                Id = Guid.NewGuid(),
                Tipo = request.Tipo,
                Numero = request.Numero.Trim(),
                FechaVencimiento = request.FechaVencimiento
            },
            cancellationToken);
        if (documento is null)
        {
            return NotFound();
        }

        return Created(
            $"/api/vehiculos/{id}/documentos",
            ToResponse(documento));
    }

    [HttpGet("{id:guid}/documentos")]
    [EndpointSummary("Listar documentos del vehiculo")]
    [EndpointDescription("Roles: Admin, Despachador. Devuelve documentos del vehiculo de la empresa.")]
    [ProducesResponseType(typeof(IEnumerable<DocumentoResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDocumentos(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetEmpresaId(out var empresaId))
        {
            return Unauthorized();
        }

        if (!HasAnyRole("Admin", "Despachador"))
        {
            return Forbid();
        }

        var vehiculo = await _vehiculoRepository.GetVehiculoByIdAsync(empresaId, id, cancellationToken);
        if (vehiculo is null)
        {
            return NotFound();
        }

        var documentos = await _documentoRepository.GetByVehiculoAsync(empresaId, id, cancellationToken);
        return Ok(documentos.Select(ToResponse));
    }

    [HttpGet("{id:guid}/asignaciones")]
    [EndpointSummary("Listar asignaciones del vehiculo")]
    [EndpointDescription("Roles: Admin, Despachador, Conductor. El conductor solo ve sus propias asignaciones sobre ese vehiculo.")]
    [ProducesResponseType(typeof(IEnumerable<AsignacionResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAsignacionesVehiculo(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetEmpresaId(out var empresaId))
        {
            return Unauthorized();
        }

        if (!HasAnyRole("Admin", "Despachador", "Conductor"))
        {
            return Forbid();
        }

        var vehiculo = await _vehiculoRepository.GetVehiculoByIdAsync(empresaId, id, cancellationToken);
        if (vehiculo is null)
        {
            return NotFound();
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

        var asignaciones = await _asignacionRepository.GetByVehiculoAsync(
            empresaId,
            id,
            conductorId,
            cancellationToken);
        return Ok(asignaciones.Select(AsignacionesController.ToResponse));
    }

    private static VehiculoResponseDTO ToResponse(Vehiculo vehiculo)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        return new VehiculoResponseDTO
        {
            Id = vehiculo.VehiculoId,
            TipoVehiculoId = vehiculo.TipoVehiculoId,
            TipoVehiculo = vehiculo.TipoVehiculo.Nombre,
            Placa = vehiculo.Placa,
            Vin = vehiculo.Vin,
            Propiedad = vehiculo.Propiedad,
            Estado = vehiculo.Estado,
            SeguroVigente = vehiculo.Documentos.Any(documento =>
                documento.Tipo == TipoDocumento.Seguro &&
                documento.FechaVencimiento >= hoy)
        };
    }

    private static DocumentoResponseDTO ToResponse(DocumentoVehiculo documento)
    {
        return new DocumentoResponseDTO
        {
            Id = documento.Id,
            VehiculoId = documento.VehiculoId,
            Tipo = documento.Tipo,
            Numero = documento.Numero,
            FechaVencimiento = documento.FechaVencimiento,
            Vencido = documento.FechaVencimiento < DateOnly.FromDateTime(DateTime.UtcNow)
        };
    }
}
