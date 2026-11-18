using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using quickgo.Application.Features.Identidad.DTOs;
using quickgo.Application.Services;
using quickgo.Domain.Constants;

namespace quickgo.API.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize(Roles = Roles.Admin)]
public class UsuariosController : ApiControllerBase {
    private readonly UsuarioService _service;
    private readonly IValidator<CrearUsuarioRequest> _crearValidator;
    private readonly IValidator<ActualizarUsuarioRequest> _actualizarValidator;

    public UsuariosController(UsuarioService service,
        IValidator<CrearUsuarioRequest> crearValidator,
        IValidator<ActualizarUsuarioRequest> actualizarValidator) {
        _service = service;
        _crearValidator = crearValidator;
        _actualizarValidator = actualizarValidator;
    }

    [HttpPost]
    [EndpointSummary("Crear un usuario")]
    [EndpointDescription("Roles: Admin. El usuario queda en la empresa del admin. Contratante solo en empresas Contratante; Despachador y Conductor solo en Transportista. Email único (409).")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear([FromBody] CrearUsuarioRequest request) {
        var invalido = await ValidarAsync(_crearValidator, request);
        if (invalido is not null) return invalido;

        return Creado(await _service.CrearAsync(request), u => $"/api/usuarios/{u.Id}");
    }

    [HttpGet]
    [EndpointSummary("Listar usuarios")]
    [EndpointDescription("Roles: Admin. Solo los usuarios de la empresa del admin.")]
    [ProducesResponseType(typeof(IEnumerable<UsuarioResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Listar() {
        return Ok(await _service.ListarAsync());
    }

    [HttpPut("{id:guid}")]
    [EndpointSummary("Editar un usuario")]
    [EndpointDescription("Roles: Admin. No cambia email ni contraseña. No se puede quitar el rol Admin al último Admin. Usuario de otra empresa: 404.")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Editar(Guid id, [FromBody] ActualizarUsuarioRequest request) {
        var invalido = await ValidarAsync(_actualizarValidator, request);
        if (invalido is not null) return invalido;

        return Responder(await _service.ActualizarAsync(id, request));
    }

    [HttpPatch("{id:guid}/desactivar")]
    [EndpointSummary("Desactivar un usuario")]
    [EndpointDescription("Roles: Admin. Baja lógica (activo = false). No puede desactivarse a sí mismo ni al último Admin. Usuario de otra empresa: 404.")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Desactivar(Guid id) {
        return Responder(await _service.DesactivarAsync(id));
    }
}
