using quickgo.Domain.entities;
namespace quickgo.Application.Interfaces;

public interface IMonedaRepository {
    Task<IEnumerable<Moneda>> GetAllAsync();
}
