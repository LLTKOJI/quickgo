
using Microsoft.EntityFrameworkCore;
using quickgo.Application.Interfaces;
using quickgo.Domain.entities;
using quickgo.Infrastructure.Data;

namespace quickgo.Infrastructure.Repositories;


public class MonedaRepository : IMonedaRepository {
    private readonly QuickGoDbContext _context;

    public MonedaRepository(QuickGoDbContext context) {
        _context = context;
    }

    public async Task<IEnumerable<Moneda>> GetAllAsync() {
        return await _context.Monedas.OrderBy(m => m.Codigo).ToListAsync();
    }
}