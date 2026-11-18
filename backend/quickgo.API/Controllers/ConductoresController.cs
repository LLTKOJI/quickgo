using quickgo.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using quickgo.Application.Features.Identidad.DTOs;
using quickgo.Application.Services;
using quickgo.Domain.Constants;

namespace quickgo.API.Controllers;


[Authorize]
public class ConductoresController : ApiControllerBase {
    private readonly ConductorService _service;
    private readonly IValidator<CrearConductorRequest> _crearValidator;
    private readonly IValidator<ActualizarConductorRequest> _actualizarValidator;
    private readonly IValidator<LicenciasPorVencerQuery> _vencerValidator;

    public ConductoresController(ConductorService service,
        IValidator<CrearConductorRequest> crearValidator,
        IValidator<ActualizarConductorRequest> actualizarValidator,
        IValidator<LicenciasPorVencerQuery> vencerValidator) {
        _service = service;
        _crearValidator = crearValidator;
        _actualizarValidator = actualizarValidator;
        _vencerValidator = vencerValidator;
    }

    [HttpPost]
    [Authorize(Roles = Roles.AdminDespachador)]
    [EndpointSummary("Registrar un conductor")]
    [EndpointDescription("Roles: Admin, Despachador. Crea el usuario (rol Conductor) y el conductor en una sola transacción. Interno: requiere email y contraseña. Externo: se crea sin acceso (usuario inactivo). Licencia única por empresa (409).")]
    [ProducesResponseType(typeof(ConductorResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear([FromBody] CrearConductorRequest request) {
        var invalido = await ValidarAsync(_crearValidator, request);
        if (invalido is not null) return invalido;

        return Creado(await _service.CrearAsync(request), c => $"/api/conductores/{c.Id}");
    }

    [HttpGet]
    [Authorize(Roles = Roles.AdminDespachadorConductor)]
    [EndpointSummary("Listar conductores")]
    [EndpointDescription("Roles: Admin, Despachador y Conductor. Filtros opcionales por estado y tipoConductor. El rol Conductor solo ve su propio perfil.")]
    [ProducesResponseType(typeof(IEnumerable<ConductorResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Listar([FromQuery] ConductorFiltroQuery filtro) {
        return Ok(await _service.ListarAsync(filtro));
    }

    // Va antes de {id:guid} por claridad; el constraint :guid evita el choque de rutas.
    [HttpGet("licencias-por-vencer")]
    [Authorize(Roles = Roles.AdminDespachador)]
    [EndpointSummary("Licencias por vencer")]
    [EndpointDescription("Roles: Admin, Despachador. Parámetro dias (1 a 365, por defecto 30). Ordenado por vence_licencia ascendente. Incluye las ya vencidas.")]
    [ProducesResponseType(typeof(IEnumerable<LicenciaPorVencerResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> LicenciasPorVencer([FromQuery] LicenciasPorVencerQuery query) {
        var invalido = await ValidarAsync(_vencerValidator, query);
        if (invalido is not null) return invalido;

        return Ok(await _service.LicenciasPorVencerAsync(query.Dias));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = Roles.AdminDespachadorConductor)]
    [EndpointSummary("Ver un conductor")]
    [EndpointDescription("Roles: Admin, Despachador y Conductor (solo el suyo). Conductor de otra empresa: 404.")]
    [ProducesResponseType(typeof(ConductorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obtener(Guid id) {
        return Responder(await _service.ObtenerAsync(id));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.AdminDespachador)]
    [EndpointSummary("Editar un conductor")]
    [EndpointDescription("Roles: Admin, Despachador. El estado cambia entre Activo e Inactivo. Licencia única por empresa (409). Conductor de otra empresa: 404.")]
    [ProducesResponseType(typeof(ConductorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Editar(Guid id, [FromBody] ActualizarConductorRequest request) {
        var invalido = await ValidarAsync(_actualizarValidator, request);
        if (invalido is not null) return invalido;

        return Responder(await _service.ActualizarAsync(id, request));
    }
}
