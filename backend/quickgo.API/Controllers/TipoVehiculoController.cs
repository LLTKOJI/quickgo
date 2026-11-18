using Microsoft.AspNetCore.Mvc;
using quickgo.Application.Features.TipoVehiculo.DTOs;
using quickgo.Application.Interfaces;

namespace quickgo.API.Controllers;

[ApiController]
[Route("api/catalogos/tipos-vehiculo")]
public class TipoVehiculoController : TenantControllerBase
{
    private readonly ITipoVehiculoRepository _tipoVehiculoRepository;

    public TipoVehiculoController(ITipoVehiculoRepository tipoVehiculoRepository)
    {
        _tipoVehiculoRepository = tipoVehiculoRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetTiposVehiculos(CancellationToken cancellationToken)
    {
        if (!HasAnyRole("Admin", "Despachador", "Conductor"))
        {
            return User.Identity?.IsAuthenticated == true ? Forbid() : Unauthorized();
        }

        var tipos = await _tipoVehiculoRepository.GetTiposVehiculosAsync(cancellationToken);
        return Ok(tipos.Select(tipo => new TipoVehiculoResponseDTO
        {
            Id = tipo.TipoVehiculoId,
            Nombre = tipo.Nombre,
            CapacidadKg = tipo.Cantidadkilogramo,
            LargoM = tipo.LargoMetros,
            AnchoM = tipo.AnchoMetros,
            AltoM = tipo.AlturaMetros,
            VolumenMaxM3 = tipo.VolumenMaximoMetrosCubicos
        }));
    }
}