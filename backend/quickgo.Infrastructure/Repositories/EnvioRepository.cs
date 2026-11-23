using Microsoft.EntityFrameworkCore;
using quickgo.Application.Interfaces;
using quickgo.Domain.entities;
using quickgo.Domain.Enums;
using quickgo.Infrastructure.Data;

namespace quickgo.Infrastructure.Repositories;

public class EnvioRepository : IEnvioRepository
{
    private readonly QuickGoDbContext _context;

    public EnvioRepository(QuickGoDbContext context)
    {
        _context = context;
    }

    public Task<Cliente?> GetClienteAsync(Guid empresaId, Guid clienteId, CancellationToken ct = default) =>
        _context.Clientes.FirstOrDefaultAsync(c => c.Id == clienteId && c.EmpresaId == empresaId, ct);

    public Task<Direccion?> GetDireccionAsync(Guid empresaId, Guid direccionId, CancellationToken ct = default) =>
        _context.Direcciones.Include(d => d.Ciudad)
            .FirstOrDefaultAsync(d => d.Id == direccionId && d.EmpresaId == empresaId, ct);

    public Task<Vehiculo?> GetVehiculoAsync(Guid empresaId, Guid vehiculoId, CancellationToken ct = default) =>
        _context.Vehiculos.FirstOrDefaultAsync(v => v.VehiculoId == vehiculoId && v.EmpresaId == empresaId, ct);

    public Task<Conductor?> GetConductorAsync(Guid empresaId, Guid conductorId, CancellationToken ct = default) =>
        _context.Conductores.Include(c => c.Usuario)
            .FirstOrDefaultAsync(c => c.Id == conductorId && c.EmpresaId == empresaId, ct);

    public Task<bool> VehiculoTieneSeguroVigenteAsync(Guid empresaId, Guid vehiculoId, DateOnly hoy, CancellationToken ct = default) =>
        _context.DocumentosVehiculo.AnyAsync(d =>
            d.EmpresaId == empresaId && d.VehiculoId == vehiculoId &&
            d.Tipo == TipoDocumento.Seguro && d.FechaVencimiento >= hoy, ct);

    public Task<bool> MonedaExisteAsync(string codigo, CancellationToken ct = default) =>
        _context.Monedas.AnyAsync(m => m.Codigo == codigo, ct);

    public async Task<Envio> AddAsync(Envio envio, CancellationToken ct = default)
    {
        _context.Envios.Add(envio);
        await _context.SaveChangesAsync(ct);
        return envio;
    }

    public Task<Envio?> GetByIdAsync(Guid empresaId, Guid envioId, CancellationToken ct = default) =>
        _context.Envios.FirstOrDefaultAsync(e => e.Id == envioId && e.EmpresaId == empresaId, ct);

    public async Task<List<Envio>> ListarAsync(Guid empresaId, EstadoEnvio? estado, DateOnly? desde, DateOnly? hasta, Guid? conductorId, CancellationToken ct = default)
    {
        var q = _context.Envios.AsNoTracking().Where(e => e.EmpresaId == empresaId);
        if (estado.HasValue) q = q.Where(e => e.Estado == estado.Value);
        if (desde.HasValue)
        {
            var d = new DateTimeOffset(desde.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            q = q.Where(e => e.FechaProgramada >= d);
        }
        if (hasta.HasValue)
        {
            var h = new DateTimeOffset(hasta.Value.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);
            q = q.Where(e => e.FechaProgramada <= h);
        }
        if (conductorId.HasValue) q = q.Where(e => e.ConductorId == conductorId.Value);
        return await q.OrderBy(e => e.FechaProgramada).ToListAsync(ct);
    }

    public async Task UpdateAsync(Envio envio, CancellationToken ct = default)
    {
        _context.Envios.Update(envio);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<Cotizacion> AddCotizacionAsync(Cotizacion cotizacion, CancellationToken ct = default)
    {
        _context.Cotizaciones.Add(cotizacion);
        await _context.SaveChangesAsync(ct);
        return cotizacion;
    }

    public Task<List<Cotizacion>> ListarCotizacionesAsync(Guid empresaId, Guid envioId, CancellationToken ct = default) =>
        _context.Cotizaciones.AsNoTracking()
            .Where(c => c.EmpresaId == empresaId && c.EnvioId == envioId)
            .OrderBy(c => c.ValidaHasta).ToListAsync(ct);

    public Task<Cotizacion?> GetCotizacionAsync(Guid empresaId, Guid envioId, Guid cotizacionId, CancellationToken ct = default) =>
        _context.Cotizaciones.FirstOrDefaultAsync(c => c.EmpresaId == empresaId && c.EnvioId == envioId && c.Id == cotizacionId, ct);

    public async Task AceptarCotizacionAsync(Envio envio, Cotizacion aceptada, List<Cotizacion> cotizaciones, CancellationToken ct = default)
    {
        foreach (var c in cotizaciones) c.Aceptada = c.Id == aceptada.Id;
        envio.Tarifa = aceptada.Monto;
        _context.Cotizaciones.UpdateRange(cotizaciones);
        _context.Envios.Update(envio);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<SeguimientoEnvio> AddSeguimientoAsync(SeguimientoEnvio seguimiento, CancellationToken ct = default)
    {
        _context.SeguimientosEnvio.Add(seguimiento);
        await _context.SaveChangesAsync(ct);
        return seguimiento;
    }

    public Task<List<SeguimientoEnvio>> ListarSeguimientoAsync(Guid empresaId, Guid envioId, CancellationToken ct = default) =>
        _context.SeguimientosEnvio.AsNoTracking()
            .Where(s => s.EmpresaId == empresaId && s.EnvioId == envioId)
            .OrderBy(s => s.Momento).ToListAsync(ct);

    public Task<SeguimientoEnvio?> GetUltimoSeguimientoAsync(Guid empresaId, Guid envioId, CancellationToken ct = default) =>
        _context.SeguimientosEnvio.AsNoTracking()
            .Where(s => s.EmpresaId == empresaId && s.EnvioId == envioId)
            .OrderByDescending(s => s.Momento).FirstOrDefaultAsync(ct);

    public async Task ActualizarEstadoVehiculoPorEnvioAsync(Envio envio, CancellationToken ct = default)
    {
        if (!envio.VehiculoId.HasValue) return;
        var vehiculo = await _context.Vehiculos.FirstOrDefaultAsync(v => v.EmpresaId == envio.EmpresaId && v.VehiculoId == envio.VehiculoId.Value, ct);
        if (vehiculo is null) return;
        if (envio.Estado == EstadoEnvio.EnCurso) vehiculo.Estado = EstadoVehiculo.EnServicio;
        else if (envio.Estado is EstadoEnvio.Completado or EstadoEnvio.Cancelado) vehiculo.Estado = EstadoVehiculo.Disponible;
        else return;
        await _context.SaveChangesAsync(ct);
    }
}
