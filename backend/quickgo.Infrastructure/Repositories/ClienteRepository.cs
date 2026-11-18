using Microsoft.EntityFrameworkCore;
using quickgo.Application.Interfaces;
using quickgo.Domain.entities;
using quickgo.Infrastructure.Data;

namespace quickgo.Infrastructure.Repositories;

public class ClienteRepository : IClienteRepository {
    private readonly QuickGoDbContext _context;

    public ClienteRepository(QuickGoDbContext context) {
        _context = context;
    }

    public async Task<IEnumerable<Cliente>> BuscarAsync(Guid empresaId, string? buscar) {
        var query = _context.Clientes.Where(c => c.EmpresaId == empresaId);

        if (!string.IsNullOrWhiteSpace(buscar)) {
            var texto = buscar.Trim().ToLower();

            query = query.Where(c =>
                c.PrimerNombre.ToLower().Contains(texto) ||
                (c.SegundoNombre != null && c.SegundoNombre.ToLower().Contains(texto)) ||
                c.PrimerApellido.ToLower().Contains(texto) ||
                (c.SegundoApellido != null && c.SegundoApellido.ToLower().Contains(texto)) ||
                c.Telefono.Contains(texto));
        }

        return await query
            .OrderBy(c => c.PrimerNombre)
            .ThenBy(c => c.PrimerApellido)
            .ToListAsync();
    }

    public async Task<Cliente?> GetByIdAsync(Guid empresaId, Guid id) {
        return await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id && c.EmpresaId == empresaId);
    }

    public async Task<Cliente> AddAsync(Cliente cliente) {
        await _context.Clientes.AddAsync(cliente);
        await _context.SaveChangesAsync();
        return cliente;
    }

    public async Task UpdateAsync(Cliente cliente) {
        _context.Clientes.Update(cliente);
        await _context.SaveChangesAsync();
    }
}