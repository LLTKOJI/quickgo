using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using quickgo.Application.Features.Envios.DTOs;
using quickgo.Application.Services;
using quickgo.Domain.Constants;

namespace quickgo.API.Controllers;

[ApiController]
[Route("api/envios")]
[Authorize]
public class EnviosController : ApiControllerBase
{
    private readonly EnvioService _service;
    private readonly IValidator<CrearEnvioRequest> _crear;
    private readonly IValidator<ActualizarEnvioRequest> _actualizar;
    private readonly IValidator<AsignarEnvioRequest> _asignar;
    private readonly IValidator<CambiarEstadoRequest> _estado;
    private readonly IValidator<CrearCotizacionRequest> _cotizacion;
    private readonly IValidator<RegistrarSeguimientoRequest> _seguimiento;

    public EnviosController(EnvioService service, IValidator<CrearEnvioRequest> crear, IValidator<ActualizarEnvioRequest> actualizar,
        IValidator<AsignarEnvioRequest> asignar, IValidator<CambiarEstadoRequest> estado, IValidator<CrearCotizacionRequest> cotizacion,
        IValidator<RegistrarSeguimientoRequest> seguimiento)
    { _service = service; _crear = crear; _actualizar = actualizar; _asignar = asignar; _estado = estado; _cotizacion = cotizacion; _seguimiento = seguimiento; }

    [HttpPost]
    [Authorize(Roles = Roles.AdminDespachador)]
    [EndpointSummary("Crear envio")]
    [EndpointDescription("Roles: Admin, Despachador. Crea un envio en estado Pendiente. Cliente, origen y destino deben pertenecer a la empresa del token.")]
    [ProducesResponseType(typeof(EnvioResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear([FromBody] CrearEnvioRequest request, CancellationToken ct)
    { var v = await ValidarAsync(_crear, request); if (v != null) return v; return Creado(await _service.CrearAsync(request, ct), r => $"/api/envios/{r.Id}"); }

    [HttpGet]
    [Authorize(Roles = Roles.AdminDespachadorConductor)]
    [EndpointSummary("Listar envios")]
    [EndpointDescription("Roles: Admin, Despachador, Conductor. El conductor solo ve envios donde es el conductor asignado. Filtros opcionales por estado, fechas y cliente.")]
    [ProducesResponseType(typeof(IEnumerable<EnvioResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Listar([FromQuery] EnvioFiltroQuery filtro, CancellationToken ct)
        => Responder(await _service.ListarAsync(filtro, ct));

    [HttpGet("{id:guid}")]
    [Authorize(Roles = Roles.AdminDespachadorConductor)]
    [EndpointSummary("Obtener envio por id")]
    [EndpointDescription("Roles: Admin, Despachador, Conductor. El conductor solo puede ver sus envios. Si es de otra empresa o no autorizado responde 404.")]
    [ProducesResponseType(typeof(EnvioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obtener(Guid id, CancellationToken ct)
        => Responder(await _service.ObtenerAsync(id, ct));

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.AdminDespachador)]
    [EndpointSummary("Actualizar envio")]
    [EndpointDescription("Roles: Admin, Despachador. Solo se puede editar un envio en estado Pendiente.")]
    [ProducesResponseType(typeof(EnvioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Actualizar(Guid id, [FromBody] ActualizarEnvioRequest request, CancellationToken ct)
    { var v = await ValidarAsync(_actualizar, request); if (v != null) return v; return Responder(await _service.ActualizarAsync(id, request, ct)); }

    [HttpPatch("{id:guid}/cancelar")]
    [Authorize(Roles = Roles.AdminDespachador)]
    [EndpointSummary("Cancelar envio")]
    [EndpointDescription("Roles: Admin, Despachador. Cancela un envio segun las reglas de negocio; no permite cancelar envios completados.")]
    [ProducesResponseType(typeof(EnvioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancelar(Guid id, CancellationToken ct)
        => Responder(await _service.CancelarAsync(id, ct));

    [HttpPatch("{id:guid}/asignar")]
    [Authorize(Roles = Roles.AdminDespachador)]
    [EndpointSummary("Asignar vehiculo y conductor al envio")]
    [EndpointDescription("Roles: Admin, Despachador. El vehiculo y conductor deben pertenecer a la empresa. Requiere seguro vigente del vehiculo.")]
    [ProducesResponseType(typeof(EnvioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Asignar(Guid id, [FromBody] AsignarEnvioRequest request, CancellationToken ct)
    { var v = await ValidarAsync(_asignar, request); if (v != null) return v; return Responder(await _service.AsignarAsync(id, request, ct)); }

    [HttpPatch("{id:guid}/estado")]
    [Authorize(Roles = Roles.AdminDespachadorConductor)]
    [EndpointSummary("Cambiar estado del envio")]
    [EndpointDescription("Roles: Admin, Despachador, Conductor. El conductor solo puede marcar EnCurso o Completado en envios asignados a el. Valida transiciones permitidas.")]
    [ProducesResponseType(typeof(EnvioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CambiarEstado(Guid id, [FromBody] CambiarEstadoRequest request, CancellationToken ct)
    { var v = await ValidarAsync(_estado, request); if (v != null) return v; return Responder(await _service.CambiarEstadoAsync(id, request, ct)); }

    [HttpPost("{id:guid}/cotizaciones")]
    [Authorize(Roles = Roles.AdminDespachador)]
    [EndpointSummary("Crear cotizacion para envio")]
    [EndpointDescription("Roles: Admin, Despachador. Registra una cotizacion para el envio indicado.")]
    [ProducesResponseType(typeof(CotizacionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CrearCotizacion(Guid id, [FromBody] CrearCotizacionRequest request, CancellationToken ct)
    { var v = await ValidarAsync(_cotizacion, request); if (v != null) return v; return Creado(await _service.CrearCotizacionAsync(id, request, ct), r => $"/api/envios/{id}/cotizaciones/{r.Id}"); }

    [HttpGet("{id:guid}/cotizaciones")]
    [Authorize(Roles = Roles.AdminDespachadorConductor)]
    [EndpointSummary("Listar cotizaciones del envio")]
    [EndpointDescription("Roles: Admin, Despachador, Conductor. Devuelve las cotizaciones del envio.")]
    [ProducesResponseType(typeof(IEnumerable<CotizacionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListarCotizaciones(Guid id, CancellationToken ct)
        => Responder(await _service.ListarCotizacionesAsync(id, ct));

    [HttpPatch("{id:guid}/cotizaciones/{cotizacionId:guid}/aceptar")]
    [Authorize(Roles = Roles.AdminDespachador)]
    [EndpointSummary("Aceptar cotizacion")]
    [EndpointDescription("Roles: Admin, Despachador. Acepta una cotizacion del envio y aplica la regla de negocio correspondiente.")]
    [ProducesResponseType(typeof(CotizacionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AceptarCotizacion(Guid id, Guid cotizacionId, CancellationToken ct)
        => Responder(await _service.AceptarCotizacionAsync(id, cotizacionId, ct));

    [HttpPost("{id:guid}/seguimiento")]
    [Authorize(Roles = Roles.Conductor)]
    [EndpointSummary("Registrar punto de seguimiento")]
    [EndpointDescription("Roles: Conductor. Solo permite registrar seguimiento en envios EnCurso asignados al conductor.")]
    [ProducesResponseType(typeof(SeguimientoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegistrarSeguimiento(Guid id, [FromBody] RegistrarSeguimientoRequest request, CancellationToken ct)
    { var v = await ValidarAsync(_seguimiento, request); if (v != null) return v; return Creado(await _service.RegistrarSeguimientoAsync(id, request, ct), r => $"/api/envios/{id}/seguimiento/{r.Id}"); }

    [HttpGet("{id:guid}/seguimiento")]
    [Authorize(Roles = Roles.AdminDespachadorConductor)]
    [EndpointSummary("Listar seguimiento del envio")]
    [EndpointDescription("Roles: Admin, Despachador, Conductor. Devuelve el historial de puntos de seguimiento del envio.")]
    [ProducesResponseType(typeof(IEnumerable<SeguimientoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListarSeguimiento(Guid id, CancellationToken ct)
        => Responder(await _service.ListarSeguimientoAsync(id, ct));

    [HttpGet("{id:guid}/seguimiento/ultimo")]
    [Authorize(Roles = Roles.AdminDespachadorConductor)]
    [EndpointSummary("Obtener ultimo seguimiento")]
    [EndpointDescription("Roles: Admin, Despachador, Conductor. Devuelve el ultimo punto de seguimiento registrado para el envio.")]
    [ProducesResponseType(typeof(SeguimientoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UltimoSeguimiento(Guid id, CancellationToken ct)
        => Responder(await _service.UltimoSeguimientoAsync(id, ct));
}
