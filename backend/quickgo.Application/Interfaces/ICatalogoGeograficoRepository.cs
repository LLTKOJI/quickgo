
using quickgo.Domain.entities;
namespace quickgo.Application.Interfaces;

public interface ICatalogoGeograficoRepository {
    Task<IEnumerable<Pais>> GetPaisesAsync();
    Task<bool> ExistePaisAsync(Guid paisId);
    Task<IEnumerable<Departamento>> GetDepartamentosAsync(Guid paisId);
    Task<bool> ExisteDepartamentoAsync(Guid departamentoId);
    Task<IEnumerable<Ciudad>> GetCiudadesAsync(Guid departamentoId);
}
