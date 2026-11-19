using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using quickgo.API.Security;
using quickgo.Application.Features.Direcciones.DTOs;
using quickgo.Application.Interfaces;
using quickgo.Domain.entities;

namespace quickgo.API.Controllers;

[ApiController]
[Route("api/direcciones")]
[Authorize(Roles = "Admin,Despachador,Contratante")]
public class DireccionesController : ControllerBase {
    private readonly IDireccionRepository _repository;
    private readonly IValidator<DireccionRequestDto> _validator;

    public DireccionesController(IDireccionRepository repository, IValidator<DireccionRequestDto> validator) {
        _repository = repository;
        _validator = validator;
    }

    [HttpGet]
    [EndpointSummary("Listar direcciones")]
    [EndpointDescription("Roles: Admin, Despachador, Contratante. Solo direcciones de la empresa del token. Filtro opcional por clienteId.")]
    [ProducesResponseType(typeof(IEnumerable<DireccionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDirecciones([FromQuery] Guid? clienteId) {
        var empresaId = ObtenerEmpresaId();
        if (empresaId == null) {
            return TokenSinEmpresa();
        }

        var direcciones = await _repository.GetAllAsync(empresaId.Value, clienteId);

        return Ok(direcciones.Select(ToResponse));
    }

    [HttpPost]
    [EndpointSummary("Registrar una dirección")]
    [EndpointDescription("Roles: Admin, Despachador, Contratante. ciudadId, callePrincipal, lat y lng obligatorios. clienteId opcional y debe ser de la empresa del token.")]
    [ProducesResponseType(typeof(DireccionResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CrearDireccion([FromBody] DireccionRequestDto request) {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid) {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToDictionary()));
        }

        var empresaId = ObtenerEmpresaId();
        if (empresaId == null) {
            return TokenSinEmpresa();
        }

        var error = await ValidarReferencias(empresaId.Value, request);
        if (error != null) {
            return error;
        }

        var nuevaDireccion = new Direccion {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId.Value
        };
        CopiarDatos(request, nuevaDireccion);

        await _repository.AddAsync(nuevaDireccion);

        var direccionCreada = await _repository.GetByIdAsync(empresaId.Value, nuevaDireccion.Id);

        return Created($"/api/direcciones/{nuevaDireccion.Id}", ToResponse(direccionCreada!));
    }

    [HttpPut("{id:guid}")]
    [EndpointSummary("Corregir una dirección")]
    [EndpointDescription("Roles: Admin, Despachador, Contratante. Si la dirección es de otra empresa responde 404.")]
    [ProducesResponseType(typeof(DireccionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EditarDireccion(Guid id, [FromBody] DireccionRequestDto request) {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid) {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToDictionary()));
        }

        var empresaId = ObtenerEmpresaId();
        if (empresaId == null) {
            return TokenSinEmpresa();
        }

        var direccion = await _repository.GetByIdAsync(empresaId.Value, id);
        if (direccion == null) {
            return Problem(statusCode: 404, title: "Dirección no encontrada", detail: "La dirección no existe.");
        }

        var error = await ValidarReferencias(empresaId.Value, request);
        if (error != null) {
            return error;
        }

        CopiarDatos(request, direccion);

        await _repository.UpdateAsync(direccion);

        var direccionEditada = await _repository.GetByIdAsync(empresaId.Value, id);

        return Ok(ToResponse(direccionEditada!));
    }

    private async Task<IActionResult?> ValidarReferencias(Guid empresaId, DireccionRequestDto request) {
        if (!await _repository.ExisteCiudadAsync(request.CiudadId!.Value)) {
            return Problem(statusCode: 400, title: "Ciudad inválida", detail: "La ciudad no existe.");
        }

        if (request.ClienteId.HasValue && !await _repository.ExisteClienteAsync(empresaId, request.ClienteId.Value)) {
            return Problem(statusCode: 400, title: "Cliente inválido", detail: "El cliente no existe.");
        }

        return null;
    }

    private Guid? ObtenerEmpresaId() {
        var claim = User.FindFirst(ClaimNames.EmpresaId);
        if (claim != null && Guid.TryParse(claim.Value, out var empresaId)) {
            return empresaId;
        }
        return null;
    }

    private IActionResult TokenSinEmpresa() {
        return Problem(statusCode: 401, title: "Token inválido", detail: "El token no tiene una empresa.");
    }

    private static void CopiarDatos(DireccionRequestDto request, Direccion direccion) {
        direccion.ClienteId = request.ClienteId;
        direccion.CiudadId = request.CiudadId!.Value;
        direccion.CallePrincipal = request.CallePrincipal.Trim();
        direccion.NumeroExterior = string.IsNullOrWhiteSpace(request.NumeroExterior) ? null : request.NumeroExterior.Trim();
        direccion.ColoniaSector = string.IsNullOrWhiteSpace(request.ColoniaSector) ? null : request.ColoniaSector.Trim();
        direccion.Referencia = string.IsNullOrWhiteSpace(request.Referencia) ? null : request.Referencia.Trim();
        direccion.Lat = request.Lat!.Value;
        direccion.Lng = request.Lng!.Value;
    }

    private static DireccionResponseDto ToResponse(Direccion d) {
        return new DireccionResponseDto {
            Id = d.Id,
            ClienteId = d.ClienteId,
            CiudadId = d.CiudadId,
            CiudadNombre = d.Ciudad?.Nombre ?? string.Empty,
            CallePrincipal = d.CallePrincipal,
            NumeroExterior = d.NumeroExterior,
            ColoniaSector = d.ColoniaSector,
            Referencia = d.Referencia,
            Lat = d.Lat,
            Lng = d.Lng
        };
    }
}
