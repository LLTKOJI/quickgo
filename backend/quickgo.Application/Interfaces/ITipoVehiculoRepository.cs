using quickgo.Domain.entities;

namespace quickgo.Application.Interfaces;

public interface ITipoVehiculoRepository
{
    Task<IReadOnlyList<TipoVehiculo>> GetTiposVehiculosAsync(CancellationToken cancellationToken = default);
}
