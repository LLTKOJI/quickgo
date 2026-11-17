using Microsoft.EntityFrameworkCore;
using quickgo.Application.Interfaces;
using quickgo.Domain.entities;
using quickgo.Infrastructure.Data;

namespace quickgo.Infrastructure.Repositories;

public class CatalogoGeograficoRepository : ICatalogoGeograficoRepository {
    private readonly QuickGoDbContext _context;

    public CatalogoGeograficoRepository(QuickGoDbContext context) {
        _context = context;
    }

    public async Task<IEnumerable<Pais>> GetPaisesAsync() {
        return await _context.Paises.OrderBy(p => p.Nombre).ToListAsync();
    }

    public async Task<bool> ExistePaisAsync(Guid paisId) {
        return await _context.Paises.AnyAsync(p => p.Id == paisId);
    }

    public async Task<IEnumerable<Departamento>> GetDepartamentosAsync(Guid paisId) {
        return await _context.Departamentos
            .Where(d => d.PaisId == paisId)
            .OrderBy(d => d.Nombre)
            .ToListAsync();
    }

    public async Task<bool> ExisteDepartamentoAsync(Guid departamentoId) {
        return await _context.Departamentos.AnyAsync(d => d.Id == departamentoId);
    }

    public async Task<IEnumerable<Ciudad>> GetCiudadesAsync(Guid departamentoId) {
        return await _context.Ciudades
            .Where(c => c.DepartamentoId == departamentoId)
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }
}
