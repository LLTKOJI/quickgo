using quickgo.Domain.entities;

namespace quickgo.Application.Interfaces;

public interface ITipoVehiculoRepository
{
    Task<IEnumerable<TipoVehiculo>> GetTiposVehiculosAsync();
    Task<TipoVehiculo> GetTipoVehiculoByIdAsync(int id);
    
    Task<TipoVehiculo>AddTipoVehiculoAsync(TipoVehiculo tipoVehiculo);
    
    Task<TipoVehiculo> UpdateTipoVehiculoAsync(TipoVehiculo tipoVehiculo);
    Task DeleteTipoVehiculoAsync(int id);
    
}