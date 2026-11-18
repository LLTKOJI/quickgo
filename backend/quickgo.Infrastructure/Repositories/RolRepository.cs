using Microsoft.EntityFrameworkCore;
using quickgo.Application.Interfaces;
using quickgo.Domain.entities;
using quickgo.Infrastructure.Data;

namespace quickgo.Infrastructure.Repositories;

public class RolRepository : IRolRepository
{
    private readonly QuickGoDbContext _context;

    public RolRepository(QuickGoDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Rol>> GetAllAsync() =>
        await _context.Roles.AsNoTracking().OrderBy(r => r.Nombre).ToListAsync();

    public Task<Rol?> GetByIdAsync(Guid id) =>
        _context.Roles.FirstOrDefaultAsync(r => r.Id == id);

    public Task<Rol?> GetByNombreAsync(string nombre) =>
        _context.Roles.FirstOrDefaultAsync(r => r.Nombre == nombre);
}
