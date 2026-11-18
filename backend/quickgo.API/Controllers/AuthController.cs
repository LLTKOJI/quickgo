using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using quickgo.Application.Features.Identidad.DTOs;
using quickgo.Application.Services;

namespace quickgo.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ApiControllerBase {
    private readonly AuthService _service;
    private readonly IValidator<LoginRequest> _validator;

    public AuthController(AuthService service, IValidator<LoginRequest> validator) {
        _service = service;
        _validator = validator;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EndpointSummary("Iniciar sesión")]
    [EndpointDescription("Roles: público. Devuelve un JWT con los claims userId, empresaId, rol y tipoEmpresa. Credenciales inválidas: 401 (mismo mensaje). Usuario o empresa inactivos: 403.")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request) {
        var invalido = await ValidarAsync(_validator, request);
        if (invalido is not null) return invalido;

        return Responder(await _service.LoginAsync(request));
    }
}
