using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using quickgo.API.Security;
using quickgo.Application.Features.Subastas;
using quickgo.Application.Features.Subastas.DTOs;
using quickgo.Application.Interfaces;
using quickgo.Application.Services;
using quickgo.Domain.entities;
using quickgo.Domain.enums;

namespace quickgo.API.Controllers;

[ApiController]
[Route("api/subastas")]
[Authorize]
public class SubastasController : ApiControllerBase {
    private readonly ISubastaRepository _repository;
    private readonly IValidator<CrearSubastaRequestDto> _validator;
    private readonly SubastaCierreService _cierreService;
    private readonly SubastaPujaService _pujaService;
    private readonly IValidator<CrearPujaRequest> _pujaValidator;
    private readonly IValidator<AdjudicarRequest> _adjudicarValidator;

    public SubastasController(ISubastaRepository repository, IValidator<CrearSubastaRequestDto> validator,
        SubastaCierreService cierreService, SubastaPujaService pujaService,
        IValidator<CrearPujaRequest> pujaValidator, IValidator<AdjudicarRequest> adjudicarValidator) {
        _repository = repository; _validator = validator; _cierreService = cierreService;
        _pujaService = pujaService; _pujaValidator = pujaValidator; _adjudicarValidator = adjudicarValidator;
    }

    [HttpPost]
    [Authorize(Roles = "Contratante")]
    [EndpointSummary("Publicar subasta")]
    [EndpointDescription("Roles: Contratante. Crea una subasta en estado Abierta. Origen/destino deben ser direcciones de la empresa del token. Tipo de vehiculo y moneda deben existir.")]
    [ProducesResponseType(typeof(SubastaResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CrearSubasta([FromBody] CrearSubastaRequestDto request) {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid) return ValidationProblem(new ValidationProblemDetails(validationResult.ToDictionary()));
        var empresaId = ObtenerEmpresaId(); if (empresaId == null) return TokenSinEmpresa();
        var moneda = request.MonedaCodigo.Trim().ToUpper();
        if (!await _repository.TipoVehiculoExisteAsync(request.TipoVehiculoId)) return Problem(statusCode: 400, title: "Datos inválidos", detail: "El tipo de vehículo no existe.");
        if (!await _repository.DireccionesSonDeEmpresaAsync(empresaId.Value, request.OrigenDireccionId, request.DestinoDireccionId)) return Problem(statusCode: 400, title: "Datos inválidos", detail: "El origen y el destino deben ser direcciones de tu empresa.");
        if (!await _repository.MonedaExisteAsync(moneda)) return Problem(statusCode: 400, title: "Datos inválidos", detail: "La moneda no existe.");
        var nueva = new Subasta { Id = Guid.NewGuid(), EmpresaId = empresaId.Value, TipoVehiculoId = request.TipoVehiculoId, OrigenDireccionId = request.OrigenDireccionId, DestinoDireccionId = request.DestinoDireccionId, Descripcion = string.IsNullOrWhiteSpace(request.Descripcion) ? null : request.Descripcion.Trim(), PesoKg = request.PesoKg, VolumenM3 = request.VolumenM3, PrecioMaximo = request.PrecioMaximo, MonedaCodigo = moneda, FechaCierre = request.FechaCierre.ToUniversalTime(), Estado = EstadoSubasta.Abierta };
        await _repository.AddAsync(nueva);
        var creada = await _repository.GetByIdAsync(empresaId.Value, nueva.Id);
        return Created($"/api/subastas/{nueva.Id}", ToResponse(creada!, 0));
    }

    [HttpGet("mias")]
    [Authorize(Roles = "Contratante")]
    [EndpointSummary("Listar mis subastas")]
    [EndpointDescription("Roles: Contratante. Devuelve subastas de la empresa del token. Filtro opcional por estado. Aplica cierre automatico de subastas vencidas antes de responder.")]
    [ProducesResponseType(typeof(IEnumerable<SubastaResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMisSubastas([FromQuery] EstadoSubasta? estado) {
        var empresaId = ObtenerEmpresaId(); if (empresaId == null) return TokenSinEmpresa();
        await _cierreService.AplicarEmpresaAsync(empresaId.Value);
        var subastas = await _repository.ListarPorEmpresaAsync(empresaId.Value, estado);
        var conteos = await _repository.ContarPujasPorSubastaAsync(subastas.Select(s => s.Id));
        return Ok(subastas.Select(s => ToResponse(s, conteos.GetValueOrDefault(s.Id, 0))));
    }

    [HttpGet("abiertas")]
    [Authorize(Roles = "Admin,Despachador")]
    [EndpointSummary("Listar subastas abiertas")]
    [EndpointDescription("Roles: Admin, Despachador (transportista). Lista subastas abiertas disponibles para pujar. Filtro opcional por tipoVehiculoId. Aplica cierre automatico de vencidas.")]
    [ProducesResponseType(typeof(IEnumerable<SubastaResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAbiertas([FromQuery] Guid? tipoVehiculoId, CancellationToken ct)
        => Responder(await _pujaService.ListarAbiertasAsync(tipoVehiculoId, ct));

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Contratante")]
    [EndpointSummary("Obtener subasta por id")]
    [EndpointDescription("Roles: Contratante dueno. Devuelve el detalle de la subasta. Si es de otra empresa responde 404.")]
    [ProducesResponseType(typeof(SubastaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSubasta(Guid id) {
        var empresaId = ObtenerEmpresaId(); if (empresaId == null) return TokenSinEmpresa();
        var subasta = await _repository.GetByIdAsync(empresaId.Value, id);
        if (subasta == null) return Problem(statusCode: 404, title: "Subasta no encontrada", detail: "La subasta no existe.");
        await _cierreService.AplicarAsync(subasta);
        var totalPujas = await _repository.ContarPujasAsync(subasta.Id);
        return Ok(ToResponse(subasta, totalPujas));
    }

    [HttpPatch("{id:guid}/cancelar")]
    [Authorize(Roles = "Contratante")]
    [EndpointSummary("Cancelar subasta")]
    [EndpointDescription("Roles: Contratante dueno. Solo se puede cancelar una subasta Abierta.")]
    [ProducesResponseType(typeof(SubastaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelarSubasta(Guid id) {
        var empresaId = ObtenerEmpresaId(); if (empresaId == null) return TokenSinEmpresa();
        var subasta = await _repository.GetByIdAsync(empresaId.Value, id);
        if (subasta == null) return Problem(statusCode: 404, title: "Subasta no encontrada", detail: "La subasta no existe.");
        await _cierreService.AplicarAsync(subasta);
        if (subasta.Estado != EstadoSubasta.Abierta) return Problem(statusCode: 409, title: "Transición inválida", detail: "Solo se puede cancelar una subasta Abierta.");
        await _repository.CancelarAsync(subasta);
        var totalPujas = await _repository.ContarPujasAsync(subasta.Id);
        return Ok(ToResponse(subasta, totalPujas));
    }

    [HttpPost("{id:guid}/pujas")]
    [Authorize(Roles = "Admin,Despachador")]
    [EndpointSummary("Crear puja")]
    [EndpointDescription("Roles: Admin, Despachador (transportista). Crea una puja en una subasta abierta. El monto debe ser menor o igual al precio maximo y menor a la mejor oferta activa.")]
    [ProducesResponseType(typeof(PujaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Pujar(Guid id, [FromBody] CrearPujaRequest request, CancellationToken ct)
    { var v = await ValidarAsync(_pujaValidator, request); if (v != null) return v; return Creado(await _pujaService.PujarAsync(id, request, ct), r => $"/api/subastas/{id}/pujas/{r.Id}"); }

    [HttpGet("{id:guid}/mejor-oferta")]
    [Authorize(Roles = "Admin,Despachador,Contratante")]
    [EndpointSummary("Obtener mejor oferta")]
    [EndpointDescription("Roles: Admin, Despachador, Contratante. Devuelve la mejor oferta activa de la subasta.")]
    [ProducesResponseType(typeof(MejorOfertaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MejorOferta(Guid id, CancellationToken ct)
        => Responder(await _pujaService.MejorOfertaAsync(id, ct));

    [HttpDelete("{id:guid}/pujas/mia")]
    [Authorize(Roles = "Admin,Despachador")]
    [EndpointSummary("Retirar mi puja")]
    [EndpointDescription("Roles: Admin, Despachador (transportista). Retira la puja activa de la empresa autenticada en la subasta.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Retirar(Guid id, CancellationToken ct)
    {
        var r = await _pujaService.RetirarAsync(id, ct);
        if (r.Exito) return NoContent();
        return Responder(r);
    }

    [HttpGet("{id:guid}/pujas")]
    [Authorize(Roles = "Contratante")]
    [EndpointSummary("Listar pujas de la subasta")]
    [EndpointDescription("Roles: Contratante dueno. Devuelve las pujas de la subasta.")]
    [ProducesResponseType(typeof(IEnumerable<PujaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerPujas(Guid id, CancellationToken ct)
        => Responder(await _pujaService.VerPujasAsync(id, ct));

    [HttpPatch("{id:guid}/adjudicar")]
    [Authorize(Roles = "Contratante")]
    [EndpointSummary("Adjudicar subasta")]
    [EndpointDescription("Roles: Contratante dueno. Adjudica la subasta a la puja ganadora. Crea el envio en la empresa transportista ganadora y marca la subasta como Adjudicada.")]
    [ProducesResponseType(typeof(SubastaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Adjudicar(Guid id, [FromBody] AdjudicarRequest request, CancellationToken ct)
    { var v = await ValidarAsync(_adjudicarValidator, request); if (v != null) return v; return Responder(await _pujaService.AdjudicarAsync(id, request, ct)); }

    private Guid? ObtenerEmpresaId() { var claim = User.FindFirst(ClaimNames.EmpresaId); return claim != null && Guid.TryParse(claim.Value, out var id) ? id : null; }
    private IActionResult TokenSinEmpresa() => Problem(statusCode: 401, title: "Token inválido", detail: "El token no tiene una empresa.");
    private static SubastaResponseDto ToResponse(Subasta s, int totalPujas) => new() { Id = s.Id, TipoVehiculoId = s.TipoVehiculoId, TipoVehiculo = s.TipoVehiculo?.Nombre ?? string.Empty, OrigenDireccionId = s.OrigenDireccionId, DestinoDireccionId = s.DestinoDireccionId, Descripcion = s.Descripcion, PesoKg = s.PesoKg, VolumenM3 = s.VolumenM3, PrecioMaximo = s.PrecioMaximo, MonedaCodigo = s.MonedaCodigo, FechaCierre = s.FechaCierre, Estado = s.Estado, TotalPujas = totalPujas, EnvioId = null };
}