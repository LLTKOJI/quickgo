using Microsoft.EntityFrameworkCore;
using quickgo.Application.Interfaces;
using quickgo.Domain.entities;
using quickgo.Infrastructure.Data;

namespace quickgo.Infrastructure.Repositories;

public class DireccionRepository : IDireccionRepository {
    private readonly QuickGoDbContext _context;

    public DireccionRepository(QuickGoDbContext context) {
        _context = context;
    }

    public async Task<IEnumerable<Direccion>> GetAllAsync(Guid empresaId, Guid? clienteId) {
        var query = _context.Direcciones
            .Include(d => d.Ciudad)
            .Where(d => d.EmpresaId == empresaId);

        if (clienteId.HasValue) {
            query = query.Where(d => d.ClienteId == clienteId.Value);
        }

        return await query.OrderBy(d => d.CallePrincipal).ToListAsync();
    }

    public async Task<Direccion?> GetByIdAsync(Guid empresaId, Guid id) {
        return await _context.Direcciones
            .Include(d => d.Ciudad)
            .FirstOrDefaultAsync(d => d.Id == id && d.EmpresaId == empresaId);
    }

    public async Task<bool> ExisteCiudadAsync(Guid ciudadId) {
        return await _context.Ciudades.AnyAsync(c => c.Id == ciudadId);
    }

    public async Task<bool> ExisteClienteAsync(Guid empresaId, Guid clienteId) {
        return await _context.Clientes.AnyAsync(c => c.Id == clienteId && c.EmpresaId == empresaId);
    }

    public async Task<Direccion> AddAsync(Direccion direccion) {
        await _context.Direcciones.AddAsync(direccion);
        await _context.SaveChangesAsync();
        return direccion;
    }

    public async Task UpdateAsync(Direccion direccion) {
        _context.Direcciones.Update(direccion);
        await _context.SaveChangesAsync();
    }
}
