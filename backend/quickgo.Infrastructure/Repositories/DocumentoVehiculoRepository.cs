using Microsoft.EntityFrameworkCore;
using quickgo.Application.Interfaces;
using quickgo.Domain.entities;
using quickgo.Infrastructure.Data;

namespace quickgo.Infrastructure.Repositories;

public class DocumentoVehiculoRepository : IDocumentoVehiculoRepository
{
    private readonly QuickGoDbContext _context;

    public DocumentoVehiculoRepository(QuickGoDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<DocumentoVehiculo>> GetByVehiculoAsync(
        Guid empresaId,
        Guid vehiculoId,
        CancellationToken cancellationToken = default)
    {
        return await _context.DocumentosVehiculo
            .AsNoTracking()
            .Where(documento =>
                documento.VehiculoId == vehiculoId &&
                documento.Vehiculo.EmpresaId == empresaId)
            .OrderBy(documento => documento.FechaVencimiento)
            .ToListAsync(cancellationToken);
    }

    public async Task<DocumentoVehiculo?> AddAsync(
        Guid empresaId,
        Guid vehiculoId,
        DocumentoVehiculo documento,
        CancellationToken cancellationToken = default)
    {
        var vehiculoExiste = await _context.Vehiculos.AnyAsync(
            vehiculo => vehiculo.EmpresaId == empresaId && vehiculo.VehiculoId == vehiculoId,
            cancellationToken);
        if (!vehiculoExiste)
        {
            return null;
        }

        documento.VehiculoId = vehiculoId;
        await _context.DocumentosVehiculo.AddAsync(documento, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return documento;
    }

    public async Task<IReadOnlyList<DocumentoVehiculo>> GetPorVencerAsync(
        Guid empresaId,
        DateOnly hasta,
        CancellationToken cancellationToken = default)
    {
        return await _context.DocumentosVehiculo
            .AsNoTracking()
            .Include(documento => documento.Vehiculo)
            .Where(documento =>
                documento.Vehiculo.EmpresaId == empresaId &&
                documento.FechaVencimiento <= hasta)
            .OrderBy(documento => documento.FechaVencimiento)
            .ToListAsync(cancellationToken);
    }
}
