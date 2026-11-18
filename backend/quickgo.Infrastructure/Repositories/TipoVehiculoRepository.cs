using Microsoft.EntityFrameworkCore;
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

    public async Task<IReadOnlyList<TipoVehiculo>> GetTiposVehiculosAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.TipoVehiculos
            .AsNoTracking()
            .OrderBy(tipo => tipo.Nombre)
            .ToListAsync(cancellationToken);
    }
}
