using quickgo.Domain.entities;

namespace quickgo.Application.Interfaces;

public interface IRolRepository {
    Task<IReadOnlyList<Rol>> GetAllAsync();
    Task<Rol?> GetByIdAsync(Guid id);
    Task<Rol?> GetByNombreAsync(string nombre);
}
