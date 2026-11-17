using Microsoft.EntityFrameworkCore;
using quickgo.Application.Interfaces;
using quickgo.Domain.entities;
using quickgo.Infrastructure.Data;

namespace quickgo.Infrastructure.Repositories;

public class FacturaRepository : IFacturaRepository {
    private readonly QuickGoDbContext _context;

    public FacturaRepository(QuickGoDbContext context) {
        _context = context;
    }

    public async Task<IEnumerable<Factura>> GetAllAsync(Guid empresaId, string? estado) {
        var query = _context.Facturas
            .Include(f => f.Pagos)
            .Where(f => f.EmpresaId == empresaId);

        if (!string.IsNullOrWhiteSpace(estado)) {
            query = query.Where(f => f.Estado == estado);
        }

        return await query.OrderByDescending(f => f.FechaEmision).ToListAsync();
    }

    public async Task<Factura?> GetByIdAsync(Guid empresaId, Guid id) {
        return await _context.Facturas
            .Include(f => f.Pagos)
            .FirstOrDefaultAsync(f => f.Id == id && f.EmpresaId == empresaId);
    }

    public async Task<Envio?> GetEnvioAsync(Guid empresaId, Guid envioId) {
        return await _context.Envios.FirstOrDefaultAsync(e => e.Id == envioId && e.EmpresaId == empresaId);
    }

    public async Task<bool> ExisteFacturaDeEnvioAsync(Guid envioId) {
        return await _context.Facturas.AnyAsync(f => f.EnvioId == envioId);
    }

    public async Task<string?> GetMonedaCotizacionAceptadaAsync(Guid envioId) {
        var cotizacion = await _context.Cotizaciones
            .FirstOrDefaultAsync(c => c.EnvioId == envioId && c.Aceptada);
        return cotizacion?.MonedaCodigo;
    }

    public async Task<bool> ExisteMonedaAsync(string codigo) {
        return await _context.Monedas.AnyAsync(m => m.Codigo == codigo);
    }

    public async Task<Factura> AddAsync(Factura factura) {
        await _context.Facturas.AddAsync(factura);
        await _context.SaveChangesAsync();
        return factura;
    }

    public async Task AgregarPagoAsync(Factura factura, Pago pago) {
        await using var transaccion = await _context.Database.BeginTransactionAsync();

        await _context.Pagos.AddAsync(pago);
        _context.Facturas.Update(factura);
        await _context.SaveChangesAsync();

        await transaccion.CommitAsync();
    }
}
