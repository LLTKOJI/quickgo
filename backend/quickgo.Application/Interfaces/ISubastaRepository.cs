using quickgo.Domain.entities;
using quickgo.Domain.enums;

namespace quickgo.Application.Interfaces;

public interface ISubastaRepository {
    Task<bool> TipoVehiculoExisteAsync(Guid tipoVehiculoId);
    Task<bool> DireccionesSonDeEmpresaAsync(Guid empresaId, Guid origenId, Guid destinoId);
    Task<bool> MonedaExisteAsync(string codigo);
    Task<Subasta> AddAsync(Subasta subasta);
    Task<Subasta?> GetByIdAsync(Guid empresaId, Guid id);
    Task<int> ContarPujasAsync(Guid subastaId);
    Task CancelarAsync(Subasta subasta);

    Task<List<Subasta>> ListarPorEmpresaAsync(Guid empresaId, EstadoSubasta? estado);
    Task<Dictionary<Guid, int>> ContarPujasPorSubastaAsync(IEnumerable<Guid> subastaIds);
    Task<List<Subasta>> ObtenerAbiertasVencidasAsync(Guid empresaId);
    Task VencerAsync(IEnumerable<Subasta> subastas);
}