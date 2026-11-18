using quickgo.Application.Features.Asignacion;
using quickgo.Domain.entities;

namespace quickgo.Application.Interfaces;

public interface IAsignacionRepository
{
    Task<Guid?> GetConductorIdByUsuarioAsync(
        Guid empresaId,
        Guid usuarioId,
        CancellationToken cancellationToken = default);
    Task<CrearAsignacionResult> CrearAsync(
        Guid empresaId,
        Guid vehiculoId,
        Guid conductorId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AsignacionVehiculoConductor>> GetByVehiculoAsync(
        Guid empresaId,
        Guid vehiculoId,
        Guid? conductorId = null,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AsignacionVehiculoConductor>> GetByConductorAsync(
        Guid empresaId,
        Guid conductorId,
        CancellationToken cancellationToken = default);
    Task<FinalizarAsignacionResult> FinalizarAsync(
        Guid empresaId,
        Guid asignacionId,
        Guid? conductorId,
        CancellationToken cancellationToken = default);
}
