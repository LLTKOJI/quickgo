using Microsoft.EntityFrameworkCore;
using quickgo.Application.Features.Vehiculo;
using quickgo.Application.Interfaces;
using quickgo.Domain.entities;
using quickgo.Domain.Enums;
using quickgo.Infrastructure.Data;

namespace quickgo.Infrastructure.Repositories;

public class VehiculoRepository : IVehiculoRepository
{
    private readonly QuickGoDbContext _context;

    public VehiculoRepository(QuickGoDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Vehiculo>> GetAllVehiculosAsync(
        Guid empresaId,
        EstadoVehiculo? estado,
        Guid? tipoVehiculoId,
        bool incluirBaja,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Vehiculos
            .AsNoTracking()
            .Include(vehiculo => vehiculo.TipoVehiculo)
            .Include(vehiculo => vehiculo.Documentos)
            .Where(vehiculo => vehiculo.EmpresaId == empresaId);

        if (!incluirBaja)
        {
            query = query.Where(vehiculo => vehiculo.Estado != EstadoVehiculo.Baja);
        }

        if (estado.HasValue)
        {
            query = query.Where(vehiculo => vehiculo.Estado == estado.Value);
        }

        if (tipoVehiculoId.HasValue)
        {
            query = query.Where(vehiculo => vehiculo.TipoVehiculoId == tipoVehiculoId.Value);
        }

        return await query.OrderBy(vehiculo => vehiculo.Placa).ToListAsync(cancellationToken);
    }

    public Task<Vehiculo?> GetVehiculoByIdAsync(
        Guid empresaId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _context.Vehiculos
            .AsNoTracking()
            .Include(vehiculo => vehiculo.TipoVehiculo)
            .Include(vehiculo => vehiculo.Documentos)
            .FirstOrDefaultAsync(
                vehiculo => vehiculo.EmpresaId == empresaId && vehiculo.VehiculoId == id,
                cancellationToken);
    }

    public Task<bool> TipoVehiculoExistsAsync(Guid tipoVehiculoId, CancellationToken cancellationToken = default)
    {
        return _context.TipoVehiculos.AnyAsync(
            tipo => tipo.TipoVehiculoId == tipoVehiculoId,
            cancellationToken);
    }

    public Task<bool> PlacaExistsAsync(
        Guid empresaId,
        string placa,
        Guid? excludingId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Vehiculos.Where(
            vehiculo => vehiculo.EmpresaId == empresaId && vehiculo.Placa == placa);

        if (excludingId.HasValue)
        {
            query = query.Where(vehiculo => vehiculo.VehiculoId != excludingId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public async Task<Vehiculo> AddVehiculoAsync(
        Vehiculo vehiculo,
        CancellationToken cancellationToken = default)
    {
        await _context.Vehiculos.AddAsync(vehiculo, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return vehiculo;
    }

    public async Task<Vehiculo?> UpdateVehiculoAsync(
        Guid empresaId,
        Guid id,
        EstadoVehiculo estado,
        string? vin,
        string? placa,
        CancellationToken cancellationToken = default)
    {
        var vehiculo = await _context.Vehiculos.FirstOrDefaultAsync(
            item => item.EmpresaId == empresaId && item.VehiculoId == id,
            cancellationToken);
        if (vehiculo is null)
        {
            return null;
        }

        vehiculo.Estado = estado;
        if (vin is not null)
        {
            vehiculo.Vin = vin;
        }

        if (placa is not null)
        {
            vehiculo.Placa = placa.Trim().ToUpperInvariant();
        }

        await _context.SaveChangesAsync(cancellationToken);
        return vehiculo;
    }

    public async Task<BajaVehiculoResult> DarDeBajaAsync(
        Guid empresaId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var vehiculo = await _context.Vehiculos.FirstOrDefaultAsync(
            item => item.EmpresaId == empresaId && item.VehiculoId == id,
            cancellationToken);
        if (vehiculo is null)
        {
            return BajaVehiculoResult.NotFound;
        }

        var tieneEnvioActivo = await _context.Envios.AnyAsync(
            envio => envio.EmpresaId == empresaId &&
                     envio.VehiculoId == id &&
                     (envio.Estado == EstadoEnvio.Asignado || envio.Estado == EstadoEnvio.EnCurso),
            cancellationToken);
        if (tieneEnvioActivo)
        {
            return BajaVehiculoResult.HasActiveShipment;
        }

        var asignacionVigente = await _context.AsignacionesVehiculoConductor.FirstOrDefaultAsync(
            asignacion => asignacion.EmpresaId == empresaId &&
                          asignacion.VehiculoId == id &&
                          asignacion.FechaFin == null,
            cancellationToken);
        if (asignacionVigente is not null)
        {
            asignacionVigente.FechaFin = DateTime.UtcNow;
        }

        vehiculo.Estado = EstadoVehiculo.Baja;
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return BajaVehiculoResult.Success;
    }
}
