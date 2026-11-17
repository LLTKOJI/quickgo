using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using quickgo.Application.Features.Facturas.DTOs;
using quickgo.Application.Interfaces;
using quickgo.Domain.enums;
using quickgo.Domain.entities;

namespace quickgo.API.Controllers;

[ApiController]
[Route("api/facturas")]
[Authorize(Roles = "Admin,Despachador")]
public class FacturasController : ControllerBase {
    private readonly IFacturaRepository _repository;
    private readonly IValidator<CreateFacturaRequestDto> _facturaValidator;
    private readonly IValidator<CreatePagoRequestDto> _pagoValidator;
    private readonly IValidator<FacturaFiltroDto> _filtroValidator;

    public FacturasController(
        IFacturaRepository repository,
        IValidator<CreateFacturaRequestDto> facturaValidator,
        IValidator<CreatePagoRequestDto> pagoValidator,
        IValidator<FacturaFiltroDto> filtroValidator) {
        _repository = repository;
        _facturaValidator = facturaValidator;
        _pagoValidator = pagoValidator;
        _filtroValidator = filtroValidator;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Generar la factura de un envío")]
    [EndpointDescription("Roles: Admin. El envío debe ser de la empresa del token y estar Completado. Una sola factura por envío. Monto = tarifa del envío. Moneda: la de la cotización aceptada; si no hay, monedaCodigo (por defecto HNL).")]
    [ProducesResponseType(typeof(FacturaDetalleResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CrearFactura([FromBody] CreateFacturaRequestDto request) {
        var validationResult = await _facturaValidator.ValidateAsync(request);
        if (!validationResult.IsValid) {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToDictionary()));
        }

        var empresaId = ObtenerEmpresaId();
        if (empresaId == null) {
            return TokenSinEmpresa();
        }

        var envio = await _repository.GetEnvioAsync(empresaId.Value, request.EnvioId!.Value);
        if (envio == null) {
            return Problem(statusCode: 404, title: "Envío no encontrado", detail: "El envío no existe.");
        }

        if (envio.Estado != "Completado") {
            return Problem(statusCode: 409, title: "Envío no completado", detail: "Solo se puede facturar un envío Completado.");
        }

        if (await _repository.ExisteFacturaDeEnvioAsync(envio.Id)) {
            return Problem(statusCode: 409, title: "Factura duplicada", detail: "El envío ya tiene una factura.");
        }

        var monedaCodigo = await _repository.GetMonedaCotizacionAceptadaAsync(envio.Id);
        if (monedaCodigo == null) {
            monedaCodigo = string.IsNullOrWhiteSpace(request.MonedaCodigo) ? "HNL" : request.MonedaCodigo.Trim().ToUpper();
        }

        if (!await _repository.ExisteMonedaAsync(monedaCodigo)) {
            return Problem(statusCode: 400, title: "Moneda inválida", detail: "La moneda no existe.");
        }

        var nuevaFactura = new Factura {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId.Value,
            EnvioId = envio.Id,
            Monto = envio.Tarifa,
            MonedaCodigo = monedaCodigo,
            Estado = EstadosFactura.Pendiente,
            FechaEmision = DateTime.UtcNow
        };

        var facturaCreada = await _repository.AddAsync(nuevaFactura);

        return CreatedAtAction(nameof(GetFactura), new { id = facturaCreada.Id }, ToDetalle(facturaCreada));
    }

    [HttpGet]
    [EndpointSummary("Consultar facturas")]
    [EndpointDescription("Roles: Admin, Despachador. Filtro opcional por estado (Pendiente o Pagada). Incluye total pagado y saldo pendiente.")]
    [ProducesResponseType(typeof(IEnumerable<FacturaResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetFacturas([FromQuery] FacturaFiltroDto filtro) {
        var validationResult = await _filtroValidator.ValidateAsync(filtro);
        if (!validationResult.IsValid) {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToDictionary()));
        }

        var empresaId = ObtenerEmpresaId();
        if (empresaId == null) {
            return TokenSinEmpresa();
        }

        var facturas = await _repository.GetAllAsync(empresaId.Value, filtro.Estado);

        var response = facturas.Select(f => new FacturaResponseDto {
            Id = f.Id,
            EnvioId = f.EnvioId,
            Monto = f.Monto,
            MonedaCodigo = f.MonedaCodigo,
            Estado = f.Estado,
            FechaEmision = f.FechaEmision,
            TotalPagado = f.Pagos.Sum(p => p.Monto),
            SaldoPendiente = f.Monto - f.Pagos.Sum(p => p.Monto)
        });

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Detalle de una factura")]
    [EndpointDescription("Roles: Admin, Despachador. Incluye los pagos, el total pagado y el saldo pendiente. Si la factura es de otra empresa responde 404.")]
    [ProducesResponseType(typeof(FacturaDetalleResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFactura(Guid id) {
        var empresaId = ObtenerEmpresaId();
        if (empresaId == null) {
            return TokenSinEmpresa();
        }

        var factura = await _repository.GetByIdAsync(empresaId.Value, id);
        if (factura == null) {
            return FacturaNoEncontrada();
        }

        return Ok(ToDetalle(factura));
    }

    [HttpPost("{id:guid}/pagos")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Registrar un pago")]
    [EndpointDescription("Roles: Admin. Método Efectivo, Transferencia o Tarjeta. El monto debe ser mayor que 0 y no exceder el saldo. Si los pagos completan el total, la factura pasa a Pagada en la misma transacción.")]
    [ProducesResponseType(typeof(FacturaDetalleResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegistrarPago(Guid id, [FromBody] CreatePagoRequestDto request) {
        var validationResult = await _pagoValidator.ValidateAsync(request);
        if (!validationResult.IsValid) {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToDictionary()));
        }

        var empresaId = ObtenerEmpresaId();
        if (empresaId == null) {
            return TokenSinEmpresa();
        }

        var factura = await _repository.GetByIdAsync(empresaId.Value, id);
        if (factura == null) {
            return FacturaNoEncontrada();
        }

        if (factura.Estado == EstadosFactura.Pagada) {
            return Problem(statusCode: 409, title: "Factura pagada", detail: "La factura ya está pagada.");
        }

        var saldo = factura.Monto - factura.Pagos.Sum(p => p.Monto);

        if (request.Monto > saldo) {
            return Problem(statusCode: 400, title: "Monto excede el saldo", detail: $"El monto no puede ser mayor que el saldo pendiente ({saldo}).");
        }

        var pago = new Pago {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId.Value,
            FacturaId = factura.Id,
            Metodo = request.Metodo,
            Monto = request.Monto,
            FechaPago = DateTime.UtcNow
        };

        if (request.Monto == saldo) {
            factura.Estado = EstadosFactura.Pagada;
        }

        await _repository.AgregarPagoAsync(factura, pago);

        var facturaActualizada = await _repository.GetByIdAsync(empresaId.Value, id);

        return CreatedAtAction(nameof(GetFactura), new { id }, ToDetalle(facturaActualizada!));
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

    private IActionResult FacturaNoEncontrada() {
        return Problem(statusCode: 404, title: "Factura no encontrada", detail: "La factura no existe.");
    }

    private static FacturaDetalleResponseDto ToDetalle(Factura f) {
        var totalPagado = f.Pagos.Sum(p => p.Monto);

        return new FacturaDetalleResponseDto {
            Id = f.Id,
            EnvioId = f.EnvioId,
            Monto = f.Monto,
            MonedaCodigo = f.MonedaCodigo,
            Estado = f.Estado,
            FechaEmision = f.FechaEmision,
            TotalPagado = totalPagado,
            SaldoPendiente = f.Monto - totalPagado,
            Pagos = f.Pagos
                .OrderBy(p => p.FechaPago)
                .Select(p => new PagoResponseDto {
                    Id = p.Id,
                    Metodo = p.Metodo,
                    Monto = p.Monto,
                    FechaPago = p.FechaPago
                })
                .ToList()
        };
    }
}
