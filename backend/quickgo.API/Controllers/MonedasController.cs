using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using quickgo.Application.Features.Catalogos.DTOs;
using quickgo.Application.Interfaces;


namespace quickgo.API.Controllers;

[ApiController]
[Route("api/catalogos")]
[Authorize]
public class MonedasController : ControllerBase {
    private readonly IMonedaRepository _repository;

    public MonedasController(IMonedaRepository repository) {
        _repository = repository;
    }

    [HttpGet("monedas")]
    [EndpointSummary("Consultar monedas")]
    [EndpointDescription("Roles: todos los usuarios autenticados. Catálogo global de solo lectura.")]
    [ProducesResponseType(typeof(IEnumerable<MonedaResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMonedas() {
        var monedas = await _repository.GetAllAsync();

        var response = monedas.Select(m => new MonedaResponseDto {
            Codigo = m.Codigo,
            Nombre = m.Nombre,
            Simbolo = m.Simbolo
        });

        return Ok(response);
    }
}
