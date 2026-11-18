using Microsoft.EntityFrameworkCore;
using quickgo.Application.Interfaces;
using quickgo.Domain.entities;
using quickgo.Domain.enums;
using quickgo.Infrastructure.Data;

namespace quickgo.Infrastructure.Repositories;

public class SubastaRepository : ISubastaRepository {
    private readonly QuickGoDbContext _context;

    public SubastaRepository(QuickGoDbContext context) {
        _context = context;
    }

    public async Task<bool> TipoVehiculoExisteAsync(Guid tipoVehiculoId) {
        return await _context.TipoVehiculos.AnyAsync(t => t.TipoVehiculoId.ToString() == tipoVehiculoId.ToString());
    }

    public async Task<bool> DireccionesSonDeEmpresaAsync(Guid empresaId, Guid origenId, Guid destinoId) {
        var ids = new[] { origenId, destinoId };
        var cantidad = await _context.Direcciones
            .Where(d => d.EmpresaId == empresaId && ids.Contains(d.Id))
            .CountAsync();
        return cantidad == 2;
    }

    public async Task<bool> MonedaExisteAsync(string codigo) {
        return await _context.Monedas.AnyAsync(m => m.Codigo == codigo);
    }

    public async Task<Subasta> AddAsync(Subasta subasta) {
        _context.Subastas.Add(subasta);
        await _context.SaveChangesAsync();
        return subasta;
    }

    public async Task<Subasta?> GetByIdAsync(Guid empresaId, Guid id) {
        return await _context.Subastas
            .Include(s => s.TipoVehiculo)
            .FirstOrDefaultAsync(s => s.Id == id && s.EmpresaId == empresaId);
    }

    public async Task<int> ContarPujasAsync(Guid subastaId) {
        return await _context.Pujas
            .IgnoreQueryFilters()
            .CountAsync(p => p.SubastaId == subastaId);
    }

    public async Task CancelarAsync(Subasta subasta) {
        var pujasActivas = await _context.Pujas
            .IgnoreQueryFilters()
            .Where(p => p.SubastaId == subasta.Id && p.Estado == EstadoPuja.Activa)
            .ToListAsync();

        foreach (var puja in pujasActivas) {
            puja.Estado = EstadoPuja.Retirada;
        }

        subasta.Estado = EstadoSubasta.Cancelada;
        await _context.SaveChangesAsync();
    }
}