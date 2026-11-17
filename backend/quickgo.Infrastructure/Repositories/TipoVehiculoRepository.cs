using quickgo.Application.Interfaces;
using quickgo.Domain.entities;
using quickgo.Infrastructure.Data;

namespace quickgo.Infrastructure.Repositories;

public class TipoVehiculoRepository : ITipoVehiculoRepository
{
    private readonly QuickGoDbContext _context;
    
    public TipoVehiculoRepository(QuickGoDbContext context)
    {
        _context = context;
    }

    public Task<IEnumerable<TipoVehiculo>> GetTiposVehiculosAsync()
    {
        throw new NotImplementedException();
    }

    public Task<TipoVehiculo> GetTipoVehiculoByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<TipoVehiculo> AddTipoVehiculoAsync(TipoVehiculo tipoVehiculo)
    {
        throw new NotImplementedException();
    }

    public Task<TipoVehiculo> UpdateTipoVehiculoAsync(TipoVehiculo tipoVehiculo)
    {
        throw new NotImplementedException();
    }

    public Task DeleteTipoVehiculoAsync(int id)
    {
        throw new NotImplementedException();
    }
}