using Microsoft.EntityFrameworkCore;
using quickgo.Application.Interfaces;
using quickgo.Domain.entities;
using quickgo.Domain.enums;
using quickgo.Infrastructure.Data;

namespace quickgo.Infrastructure.Repositories;

public class ConductorRepository : IConductorRepository
{
    private readonly QuickGoDbContext _context;

    public ConductorRepository(QuickGoDbContext context)
    {
        _context = context;
    }

    public Task<Conductor?> GetByIdAsync(Guid id) =>
        _context.Conductores.Include(c => c.Usuario).FirstOrDefaultAsync(c => c.Id == id);

    public async Task<IReadOnlyList<Conductor>> ListarAsync(EstadoConductor? estado, TipoConductor? tipo, Guid? soloId)
    {
        var query = _context.Conductores.AsNoTracking().Include(c => c.Usuario).AsQueryable();

        if (estado.HasValue) query = query.Where(c => c.Estado == estado.Value);
        if (tipo.HasValue) query = query.Where(c => c.TipoConductor == tipo.Value);
        if (soloId.HasValue) query = query.Where(c => c.Id == soloId.Value);

        return await query
            .OrderBy(c => c.Usuario!.PrimerNombre).ThenBy(c => c.Usuario!.PrimerApellido)
            .ToListAsync();
    }

    public Task<bool> ExisteLicenciaAsync(string licencia, Guid? excluirId) =>
        _context.Conductores.AnyAsync(c => c.Licencia == licencia && (excluirId == null || c.Id != excluirId));

    public async Task<IReadOnlyList<Conductor>> ListarConLicenciaHastaAsync(DateOnly limite) =>
        await _context.Conductores.AsNoTracking().Include(c => c.Usuario)
            .Where(c => c.VenceLicencia <= limite)
            .OrderBy(c => c.VenceLicencia)
            .ToListAsync();

    public void Add(Conductor conductor) => _context.Conductores.Add(conductor);
}
