using quickgo.Domain.entities;

namespace quickgo.Application.Interfaces;

public interface ISubastaRepository {
    Task<bool> TipoVehiculoExisteAsync(Guid tipoVehiculoId);
    Task<bool> DireccionesSonDeEmpresaAsync(Guid empresaId, Guid origenId, Guid destinoId);
    Task<bool> MonedaExisteAsync(string codigo);
    Task<Subasta> AddAsync(Subasta subasta);
    Task<Subasta?> GetByIdAsync(Guid empresaId, Guid id);
    Task<int> ContarPujasAsync(Guid subastaId);
    Task CancelarAsync(Subasta subasta);
}