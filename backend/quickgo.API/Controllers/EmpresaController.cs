using Microsoft.AspNetCore.Mvc;
using quickgo.Application.Features.Empresa.DTOs;
using quickgo.Application.Interfaces;
using quickgo.Domain.entities;

namespace quickgo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmpresaController : ControllerBase
{
    private readonly IEmpresaRepository _empresaRepository;

    public EmpresaController(IEmpresaRepository empresaRepository)
    {
        _empresaRepository = empresaRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetEmpresas()
    {
        var accessResult = RequireAdmin();
        if (accessResult is not null)
        {
            return accessResult;
        }

        var empresas = await _empresaRepository.GetAllEmpresasAsync();
        return Ok(empresas.Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetEmpresa(Guid id)
    {
        var accessResult = RequireAdmin();
        if (accessResult is not null)
        {
            return accessResult;
        }

        var empresa = await _empresaRepository.GetEmpresaAsync(id);
        return empresa is null ? NotFound() : Ok(ToResponse(empresa));
    }

    [HttpPost]
    public async Task<IActionResult> CreateEmpresa(CreateEmpresaRequestDTO request)
    {
        var accessResult = RequireAdmin();
        if (accessResult is not null)
        {
            return accessResult;
        }

        var empresa = await _empresaRepository.AddEmpresaAsync(new Empresa
        {
            NombreComercial = request.NombreComercial,
            RazonSocial = request.RazonSocial,
            RTN = request.RTN,
            TipoComercial = request.TipoComercial,
            Plan = request.Plan,
            Activa = request.Activa
        });

        return CreatedAtAction(nameof(GetEmpresa), new { id = empresa.EmpresaId }, ToResponse(empresa));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateEmpresa(Guid id, CreateEmpresaRequestDTO request)
    {
        var accessResult = RequireAdmin();
        if (accessResult is not null)
        {
            return accessResult;
        }

        var empresa = await _empresaRepository.UpdateEmpresaAsync(new Empresa
        {
            EmpresaId = id,
            NombreComercial = request.NombreComercial,
            RazonSocial = request.RazonSocial,
            RTN = request.RTN,
            TipoComercial = request.TipoComercial,
            Plan = request.Plan,
            Activa = request.Activa
        });

        return empresa is null ? NotFound() : Ok(ToResponse(empresa));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteEmpresa(Guid id)
    {
        var accessResult = RequireAdmin();
        if (accessResult is not null)
        {
            return accessResult;
        }

        var deleted = await _empresaRepository.DeleteEmpresaAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    private IActionResult? RequireAdmin()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Unauthorized();
        }

        return User.IsInRole("Admin") ? null : Forbid();
    }

    private static EmpresaResponseDTO ToResponse(Empresa empresa) => new()
    {
        EmpresaId = empresa.EmpresaId,
        NombreComercial = empresa.NombreComercial,
        RazonSocial = empresa.RazonSocial,
        RTN = empresa.RTN,
        TipoComercial = empresa.TipoComercial,
        Plan = empresa.Plan,
        Activa = empresa.Activa
    };
}