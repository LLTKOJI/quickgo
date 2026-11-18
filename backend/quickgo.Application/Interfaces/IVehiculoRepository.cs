using quickgo.Application.Features.Vehiculo;
using quickgo.Domain.entities;
using quickgo.Domain.Enums;

namespace quickgo.Application.Interfaces;

public interface IVehiculoRepository
{
    Task<IReadOnlyList<Vehiculo>> GetAllVehiculosAsync(
        Guid empresaId,
        EstadoVehiculo? estado,
        Guid? tipoVehiculoId,
        bool incluirBaja,
        CancellationToken cancellationToken = default);
    Task<Vehiculo?> GetVehiculoByIdAsync(Guid empresaId, Guid id, CancellationToken cancellationToken = default);
    Task<bool> TipoVehiculoExistsAsync(Guid tipoVehiculoId, CancellationToken cancellationToken = default);
    Task<bool> PlacaExistsAsync(Guid empresaId, string placa, Guid? excludingId = null, CancellationToken cancellationToken = default);
    Task<Vehiculo> AddVehiculoAsync(Vehiculo vehiculo, CancellationToken cancellationToken = default);
    Task<Vehiculo?> UpdateVehiculoAsync(
        Guid empresaId,
        Guid id,
        EstadoVehiculo estado,
        string? vin,
        string? placa,
        CancellationToken cancellationToken = default);
    Task<BajaVehiculoResult> DarDeBajaAsync(Guid empresaId, Guid id, CancellationToken cancellationToken = default);
}
