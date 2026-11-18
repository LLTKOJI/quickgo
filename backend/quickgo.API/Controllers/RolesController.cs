using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using quickgo.Application.Features.Identidad.DTOs;
using quickgo.Application.Services;
using quickgo.Domain.Constants;

namespace quickgo.API.Controllers;

[ApiController]
[Route("api/catalogos")]
[Authorize(Roles = Roles.Admin)]
public class RolesController : ControllerBase {
    private readonly RolService _service;

    public RolesController(RolService service) {
        _service = service;
    }

    [HttpGet("roles")]
    [EndpointSummary("Consultar roles")]
    [EndpointDescription("Roles: Admin. Catálogo global de solo lectura (Admin, Despachador, Conductor, Contratante).")]
    [ProducesResponseType(typeof(IEnumerable<RolResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRoles() {
        return Ok(await _service.ListarAsync());
    }
}
