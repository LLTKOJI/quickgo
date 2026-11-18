using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using quickgo.Application.Features.Clientes.DTOs;
using quickgo.Application.Interfaces;
using quickgo.Domain.entities;
using quickgo.Domain.Constants;

namespace quickgo.API.Controllers;

[ApiController]
[Route("api/clientes")]
[Authorize(Roles = Roles.AdminDespachador)]
public class ClientesController : ControllerBase {
    private readonly IClienteRepository _repository;
    private readonly IValidator<ClienteRequestDto> _validator;
    private readonly ITenantContext _tenant;

    public ClientesController(IClienteRepository repository, IValidator<ClienteRequestDto> validator, ITenantContext tenant) {
        _repository = repository;
        _validator = validator;
        _tenant = tenant;
    }

    [HttpGet]
    [EndpointSummary("Buscar clientes")]
    [EndpointDescription("Roles: Admin, Despachador. Búsqueda parcial sin distinguir mayúsculas por nombres, apellidos y teléfono. Sin 'buscar' devuelve todos los clientes de la empresa.")]
    [ProducesResponseType(typeof(IEnumerable<ClienteResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetClientes([FromQuery] string? buscar) {
        var empresaId = ObtenerEmpresaId();
        if (empresaId == null) {
            return TokenSinEmpresa();
        }

        var clientes = await _repository.BuscarAsync(empresaId.Value, buscar);

        return Ok(clientes.Select(ToResponse));
    }

    [HttpPost]
    [EndpointSummary("Registrar un cliente")]
    [EndpointDescription("Roles: Admin, Despachador. Tipo Empresa o Particular. La empresa se toma del token, no del cuerpo.")]
    [ProducesResponseType(typeof(ClienteResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CrearCliente([FromBody] ClienteRequestDto request) {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid) {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToDictionary()));
        }

        var empresaId = ObtenerEmpresaId();
        if (empresaId == null) {
            return TokenSinEmpresa();
        }

        var nuevoCliente = new Cliente {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId.Value
        };
        CopiarDatos(request, nuevoCliente);

        var clienteCreado = await _repository.AddAsync(nuevoCliente);

        return Created($"/api/clientes/{clienteCreado.Id}", ToResponse(clienteCreado));
    }

    [HttpPut("{id:guid}")]
    [EndpointSummary("Editar un cliente")]
    [EndpointDescription("Roles: Admin, Despachador. Si el cliente es de otra empresa responde 404.")]
    [ProducesResponseType(typeof(ClienteResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EditarCliente(Guid id, [FromBody] ClienteRequestDto request) {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid) {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToDictionary()));
        }

        var empresaId = ObtenerEmpresaId();
        if (empresaId == null) {
            return TokenSinEmpresa();
        }

        var cliente = await _repository.GetByIdAsync(empresaId.Value, id);
        if (cliente == null) {
            return Problem(statusCode: 404, title: "Cliente no encontrado", detail: "El cliente no existe.");
        }

        CopiarDatos(request, cliente);

        await _repository.UpdateAsync(cliente);

        return Ok(ToResponse(cliente));
    }

    private Guid? ObtenerEmpresaId() {
        return _tenant.EmpresaId;
    }

    private IActionResult TokenSinEmpresa() {
        return Problem(statusCode: 401, title: "Token inválido", detail: "El token no tiene una empresa.");
    }

    private static void CopiarDatos(ClienteRequestDto request, Cliente cliente) {
        cliente.PrimerNombre = request.PrimerNombre.Trim();
        cliente.SegundoNombre = string.IsNullOrWhiteSpace(request.SegundoNombre) ? null : request.SegundoNombre.Trim();
        cliente.PrimerApellido = request.PrimerApellido.Trim();
        cliente.SegundoApellido = string.IsNullOrWhiteSpace(request.SegundoApellido) ? null : request.SegundoApellido.Trim();
        cliente.Telefono = request.Telefono.Trim();
        cliente.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim().ToLower();
        cliente.TipoCliente = request.TipoCliente;
    }

    private static ClienteResponseDto ToResponse(Cliente c) {
        return new ClienteResponseDto {
            Id = c.Id,
            PrimerNombre = c.PrimerNombre,
            SegundoNombre = c.SegundoNombre,
            PrimerApellido = c.PrimerApellido,
            SegundoApellido = c.SegundoApellido,
            Telefono = c.Telefono,
            Email = c.Email,
            TipoCliente = c.TipoCliente
        };
    }
}
