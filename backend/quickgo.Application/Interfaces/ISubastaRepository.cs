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
    Task<List<Subasta>> ObtenerAbiertasVencidasGlobalAsync();
    Task VencerAsync(IEnumerable<Subasta> subastas);

    Task<List<Subasta>> ListarAbiertasParaEmpresaAsync(Guid empresaId, Guid? tipoVehiculoId);
    Task<Dictionary<Guid, decimal>> ObtenerMejoresOfertasAsync(IEnumerable<Guid> subastaIds);
    Task<HashSet<Guid>> ObtenerSubastasPujadasPorEmpresaAsync(Guid empresaId, IEnumerable<Guid> subastaIds);
    Task<Subasta?> GetAbiertaByIdAsync(Guid id);
    Task<bool> EmpresaTieneVehiculoElegibleAsync(Guid empresaId, Guid tipoVehiculoId, DateOnly hoy);
    Task<Conductor?> GetConductorActivoDeEmpresaAsync(Guid empresaId, Guid conductorId);
    Task<Puja?> GetPujaEmpresaAsync(Guid subastaId, Guid empresaId);
    Task<Puja> AddPujaAsync(Puja puja);
    Task<Puja?> GetPujaByIdAsync(Guid pujaId);
    Task<List<Puja>> ListarPujasSubastaAsync(Guid subastaId);
    Task SaveChangesAsync();
    Task<Subasta?> GetByIdConDetalleAsync(Guid empresaId, Guid id);
    Task<Empresa?> GetEmpresaAsync(Guid empresaId);
    Task<Guid> AdjudicarAsync(Subasta subasta, Puja ganadora, List<Puja> pujas, Guid empresaGanadora, string clienteNombre);
}