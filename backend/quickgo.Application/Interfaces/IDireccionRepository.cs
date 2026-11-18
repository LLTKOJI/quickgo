using quickgo.Domain.entities;
namespace quickgo.Application.Interfaces;

public interface IDireccionRepository {
    Task<IEnumerable<Direccion>> GetAllAsync(Guid empresaId, Guid? clienteId);
    Task<Direccion?> GetByIdAsync(Guid empresaId, Guid id);
    Task<bool> ExisteCiudadAsync(Guid ciudadId);
    Task<bool> ExisteClienteAsync(Guid empresaId, Guid clienteId);
    Task<Direccion> AddAsync(Direccion direccion);
    Task UpdateAsync(Direccion direccion);
}
