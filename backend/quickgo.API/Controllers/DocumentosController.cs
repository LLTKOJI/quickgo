using Microsoft.AspNetCore.Mvc;
using quickgo.Application.Features.Vehiculo.DTOs;
using quickgo.Application.Interfaces;
using quickgo.Domain.Enums;

namespace quickgo.API.Controllers;

[ApiController]
[Route("api/documentos")]
public class DocumentosController : TenantControllerBase
{
    private readonly IDocumentoVehiculoRepository _documentoRepository;

    public DocumentosController(IDocumentoVehiculoRepository documentoRepository)
    {
        _documentoRepository = documentoRepository;
    }

    [HttpGet("por-vencer")]
    public async Task<IActionResult> GetPorVencer(
        [FromQuery] DocumentosPorVencerQuery query,
        CancellationToken cancellationToken)
    {
        if (!TryGetEmpresaId(out var empresaId))
        {
            return Unauthorized();
        }

        if (!HasAnyRole("Admin", "Despachador"))
        {
            return Forbid();
        }

        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var documentos = await _documentoRepository.GetPorVencerAsync(
            empresaId,
            hoy.AddDays(query.Dias),
            cancellationToken);

        return Ok(documentos.Select(documento => new DocumentoPorVencerResponseDTO
        {
            DocumentoId = documento.Id,
            VehiculoId = documento.VehiculoId,
            Placa = documento.Vehiculo.Placa,
            Tipo = documento.Tipo,
            Numero = documento.Numero,
            FechaVencimiento = documento.FechaVencimiento,
            DiasRestantes = documento.FechaVencimiento.DayNumber - hoy.DayNumber,
            Vencido = documento.FechaVencimiento < hoy
        }));
    }
}
