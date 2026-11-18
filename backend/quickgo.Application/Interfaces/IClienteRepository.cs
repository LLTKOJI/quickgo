using quickgo.Domain.entities;
namespace quickgo.Application.Interfaces;

public interface IClienteRepository {
    Task<IEnumerable<Cliente>> BuscarAsync(Guid empresaId, string? buscar);
    Task<Cliente?> GetByIdAsync(Guid empresaId, Guid id);
    Task<Cliente> AddAsync(Cliente cliente);
    Task UpdateAsync(Cliente cliente);
}
