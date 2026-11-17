using quickgo.Domain.entities;

namespace quickgo.Application.Interfaces;

public interface IVehiculoRepository
{
    Task<IEnumerable<Vehiculo>> GetAllVehiculosAsync();
    Task<Vehiculo?> GetVehiculoByIdAsync(int id);
    Task<Vehiculo> AddVehiculoAsync(Vehiculo vehiculo);
    Task<Vehiculo> UpdateVehiculoAsync(Vehiculo vehiculo);
    
    Task <Vehiculo?>DeleteVehiculoAsync(int id);
    
}