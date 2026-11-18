using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using quickgo.Application.Features.Catalogos.DTOs;
using quickgo.Application.Interfaces;

namespace quickgo.API.Controllers;

[ApiController]
[Route("api/catalogos")]
[Authorize]
public class CatalogosGeograficosController : ControllerBase {
    private readonly ICatalogoGeograficoRepository _repository;
    private readonly IValidator<DepartamentosFiltroDto> _departamentosValidator;
    private readonly IValidator<CiudadesFiltroDto> _ciudadesValidator;

    public CatalogosGeograficosController(
        ICatalogoGeograficoRepository repository,
        IValidator<DepartamentosFiltroDto> departamentosValidator,
        IValidator<CiudadesFiltroDto> ciudadesValidator) {
        _repository = repository;
        _departamentosValidator = departamentosValidator;
        _ciudadesValidator = ciudadesValidator;
    }

    [HttpGet("paises")]
    [EndpointSummary("Consultar países")]
    [EndpointDescription("Roles: todos los usuarios autenticados. Catálogo global de solo lectura.")]
    [ProducesResponseType(typeof(IEnumerable<PaisResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPaises() {
        var paises = await _repository.GetPaisesAsync();

        var response = paises.Select(p => new PaisResponseDto {
            Id = p.Id,
            Nombre = p.Nombre,
            CodigoIso = p.CodigoIso
        });

        return Ok(response);
    }

    [HttpGet("departamentos")]
    [EndpointSummary("Consultar departamentos de un país")]
    [EndpointDescription("Roles: todos los usuarios autenticados. Requiere paisId. Catálogo global de solo lectura.")]
    [ProducesResponseType(typeof(IEnumerable<DepartamentoResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDepartamentos([FromQuery] DepartamentosFiltroDto filtro) {
        var validationResult = await _departamentosValidator.ValidateAsync(filtro);
        if (!validationResult.IsValid) {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToDictionary()));
        }

        if (!await _repository.ExistePaisAsync(filtro.PaisId!.Value)) {
            return Problem(statusCode: 404, title: "País no encontrado", detail: "El país no existe.");
        }

        var departamentos = await _repository.GetDepartamentosAsync(filtro.PaisId.Value);

        var response = departamentos.Select(d => new DepartamentoResponseDto {
            Id = d.Id,
            PaisId = d.PaisId,
            Nombre = d.Nombre
        });

        return Ok(response);
    }

    [HttpGet("ciudades")]
    [EndpointSummary("Consultar ciudades de un departamento")]
    [EndpointDescription("Roles: todos los usuarios autenticados. Requiere departamentoId. Incluye código postal. Catálogo global de solo lectura.")]
    [ProducesResponseType(typeof(IEnumerable<CiudadResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCiudades([FromQuery] CiudadesFiltroDto filtro) {
        var validationResult = await _ciudadesValidator.ValidateAsync(filtro);
        if (!validationResult.IsValid) {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToDictionary()));
        }

        if (!await _repository.ExisteDepartamentoAsync(filtro.DepartamentoId!.Value)) {
            return Problem(statusCode: 404, title: "Departamento no encontrado", detail: "El departamento no existe.");
        }

        var ciudades = await _repository.GetCiudadesAsync(filtro.DepartamentoId.Value);

        var response = ciudades.Select(c => new CiudadResponseDto {
            Id = c.Id,
            DepartamentoId = c.DepartamentoId,
            Nombre = c.Nombre,
            CodigoPostal = c.CodigoPostal
        });

        return Ok(response);
    }
}
