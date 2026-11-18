using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using quickgo.Application.Features.Subastas.DTOs;
using quickgo.Application.Interfaces;
using quickgo.Domain.entities;
using quickgo.Domain.enums;

namespace quickgo.API.Controllers;

[ApiController]
[Route("api/subastas")]
[Authorize(Roles = "Contratante")]
public class SubastasController : ControllerBase {
    private readonly ISubastaRepository _repository;
    private readonly IValidator<CrearSubastaRequestDto> _validator;

    public SubastasController(ISubastaRepository repository, IValidator<CrearSubastaRequestDto> validator) {
        _repository = repository;
        _validator = validator;
    }

    [HttpPost]
    [EndpointSummary("Publicar una subasta")]
    [EndpointDescription("Rol: Contratante. Origen y destino deben ser direcciones de la empresa. Estado inicial Abierta. La empresa se toma del token.")]
    [ProducesResponseType(typeof(SubastaResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CrearSubasta([FromBody] CrearSubastaRequestDto request) {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid) {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToDictionary()));
        }

        var empresaId = ObtenerEmpresaId();
        if (empresaId == null) {
            return TokenSinEmpresa();
        }

        var moneda = request.MonedaCodigo.Trim().ToUpper();

        if (!await _repository.TipoVehiculoExisteAsync(request.TipoVehiculoId)) {
            return Problem(statusCode: 400, title: "Datos inválidos", detail: "El tipo de vehículo no existe.");
        }

        if (!await _repository.DireccionesSonDeEmpresaAsync(empresaId.Value, request.OrigenDireccionId, request.DestinoDireccionId)) {
            return Problem(statusCode: 400, title: "Datos inválidos", detail: "El origen y el destino deben ser direcciones de tu empresa.");
        }

        if (!await _repository.MonedaExisteAsync(moneda)) {
            return Problem(statusCode: 400, title: "Datos inválidos", detail: "La moneda no existe.");
        }

        var nueva = new Subasta {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId.Value,
            TipoVehiculoId = request.TipoVehiculoId,
            OrigenDireccionId = request.OrigenDireccionId,
            DestinoDireccionId = request.DestinoDireccionId,
            Descripcion = string.IsNullOrWhiteSpace(request.Descripcion) ? null : request.Descripcion.Trim(),
            PesoKg = request.PesoKg,
            VolumenM3 = request.VolumenM3,
            PrecioMaximo = request.PrecioMaximo,
            MonedaCodigo = moneda,
            FechaCierre = request.FechaCierre.ToUniversalTime(),
            Estado = EstadoSubasta.Abierta
        };

        await _repository.AddAsync(nueva);

        var creada = await _repository.GetByIdAsync(empresaId.Value, nueva.Id);

        return Created($"/api/subastas/{nueva.Id}", ToResponse(creada!, 0));
    }

    [HttpPatch("{id:guid}/cancelar")]
    [EndpointSummary("Cancelar una subasta")]
    [EndpointDescription("Rol: Contratante dueño. Solo si está Abierta. Las pujas Activas pasan a Retirada. Otra empresa responde 404.")]
    [ProducesResponseType(typeof(SubastaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelarSubasta(Guid id) {
        var empresaId = ObtenerEmpresaId();
        if (empresaId == null) {
            return TokenSinEmpresa();
        }

        var subasta = await _repository.GetByIdAsync(empresaId.Value, id);
        if (subasta == null) {
            return Problem(statusCode: 404, title: "Subasta no encontrada", detail: "La subasta no existe.");
        }

        if (subasta.Estado != EstadoSubasta.Abierta) {
            return Problem(statusCode: 409, title: "Transición inválida", detail: "Solo se puede cancelar una subasta Abierta.");
        }

        await _repository.CancelarAsync(subasta);
        var totalPujas = await _repository.ContarPujasAsync(subasta.Id);

        return Ok(ToResponse(subasta, totalPujas));
    }

    private Guid? ObtenerEmpresaId() {
        var claim = User.FindFirst("empresa_id");
        if (claim != null && Guid.TryParse(claim.Value, out var empresaId)) {
            return empresaId;
        }
        return null;
    }

    private IActionResult TokenSinEmpresa() {
        return Problem(statusCode: 401, title: "Token inválido", detail: "El token no tiene una empresa.");
    }

    private static SubastaResponseDto ToResponse(Subasta s, int totalPujas) {
        return new SubastaResponseDto {
            Id = s.Id,
            TipoVehiculoId = s.TipoVehiculoId,
            TipoVehiculo = s.TipoVehiculo?.Nombre ?? string.Empty,
            OrigenDireccionId = s.OrigenDireccionId,
            DestinoDireccionId = s.DestinoDireccionId,
            Descripcion = s.Descripcion,
            PesoKg = s.PesoKg,
            VolumenM3 = s.VolumenM3,
            PrecioMaximo = s.PrecioMaximo,
            MonedaCodigo = s.MonedaCodigo,
            FechaCierre = s.FechaCierre,
            Estado = s.Estado,
            TotalPujas = totalPujas,
            EnvioId = null
        };
    }
}