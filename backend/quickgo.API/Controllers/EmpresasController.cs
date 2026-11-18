using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using quickgo.Application.Features.Identidad.DTOs;
using quickgo.Application.Services;

namespace quickgo.API.Controllers;

[ApiController]
[Route("api/empresas")]
public class EmpresasController : ApiControllerBase {
    private readonly EmpresaService _service;
    private readonly IValidator<RegistroEmpresaRequest> _validator;

    public EmpresasController(EmpresaService service, IValidator<RegistroEmpresaRequest> validator) {
        _service = service;
        _validator = validator;
    }

    [HttpPost("registro")]
    [AllowAnonymous]
    [EndpointSummary("Registrar una empresa")]
    [EndpointDescription("Roles: público. Crea la empresa (Transportista o Contratante) y su usuario Admin en una sola transacción. RTN, slug y correo únicos (409).")]
    [ProducesResponseType(typeof(EmpresaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Registro([FromBody] RegistroEmpresaRequest request) {
        var invalido = await ValidarAsync(_validator, request);
        if (invalido is not null) return invalido;

        var resultado = await _service.RegistrarAsync(request);
        return Creado(resultado, e => $"/api/empresas/{e.Id}");
    }
}
