using quickgo.Domain.entities;

namespace quickgo.Application.Interfaces;

public interface IDocumentoVehiculoRepository
{
    Task<IReadOnlyList<DocumentoVehiculo>> GetByVehiculoAsync(
        Guid empresaId,
        Guid vehiculoId,
        CancellationToken cancellationToken = default);
    Task<DocumentoVehiculo?> AddAsync(
        Guid empresaId,
        Guid vehiculoId,
        DocumentoVehiculo documento,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentoVehiculo>> GetPorVencerAsync(
        Guid empresaId,
        DateOnly hasta,
        CancellationToken cancellationToken = default);
}
