using Microsoft.EntityFrameworkCore;
using quickgo.Application.Features.Asignacion;
using quickgo.Application.Interfaces;
using quickgo.Domain.entities;
using quickgo.Domain.Enums;
using quickgo.Infrastructure.Data;

namespace quickgo.Infrastructure.Repositories;

public class AsignacionRepository : IAsignacionRepository
{
    private readonly QuickGoDbContext _context;

    public AsignacionRepository(QuickGoDbContext context)
    {
        _context = context;
    }

    public async Task<Guid?> GetConductorIdByUsuarioAsync(
        Guid empresaId,
        Guid usuarioId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Conductores
            .Where(conductor => conductor.EmpresaId == empresaId && conductor.UsuarioId == usuarioId)
            .Select(conductor => (Guid?)conductor.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<CrearAsignacionResult> CrearAsync(
        Guid empresaId,
        Guid vehiculoId,
        Guid conductorId,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var vehiculo = await _context.Vehiculos.FirstOrDefaultAsync(
            item => item.EmpresaId == empresaId && item.VehiculoId == vehiculoId,
            cancellationToken);
        if (vehiculo is null)
        {
            return CrearAsignacionResult.VehicleNotFound;
        }

        if (vehiculo.Estado == EstadoVehiculo.Baja)
        {
            return CrearAsignacionResult.VehicleUnavailable;
        }

        var conductor = await _context.Conductores
            .Include(item => item.Usuario)
            .FirstOrDefaultAsync(
                item => item.EmpresaId == empresaId && item.Id == conductorId,
                cancellationToken);
        if (conductor is null)
        {
            return CrearAsignacionResult.ConductorNotFound;
        }

        if (!conductor.Activo || !conductor.Usuario.Activo)
        {
            return CrearAsignacionResult.ConductorInactive;
        }

        var ahora = DateTime.UtcNow;
        var asignacionVigente = await _context.AsignacionesVehiculoConductor.FirstOrDefaultAsync(
            asignacion => asignacion.EmpresaId == empresaId &&
                          asignacion.VehiculoId == vehiculoId &&
                          asignacion.FechaFin == null,
            cancellationToken);
        if (asignacionVigente is not null)
        {
            asignacionVigente.FechaFin = ahora;
            await _context.SaveChangesAsync(cancellationToken);
        }

        await _context.AsignacionesVehiculoConductor.AddAsync(
            new AsignacionVehiculoConductor
            {
                Id = Guid.NewGuid(),
                EmpresaId = empresaId,
                VehiculoId = vehiculoId,
                ConductorId = conductorId,
                FechaInicio = ahora
            },
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return CrearAsignacionResult.Success;
    }

    public async Task<IReadOnlyList<AsignacionVehiculoConductor>> GetByVehiculoAsync(
        Guid empresaId,
        Guid vehiculoId,
        Guid? conductorId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AsignacionesVehiculoConductor
            .AsNoTracking()
            .Include(asignacion => asignacion.Vehiculo)
            .Include(asignacion => asignacion.Conductor)
                .ThenInclude(conductor => conductor.Usuario)
            .Where(asignacion =>
                asignacion.EmpresaId == empresaId &&
                asignacion.VehiculoId == vehiculoId);

        if (conductorId.HasValue)
        {
            query = query.Where(asignacion => asignacion.ConductorId == conductorId.Value);
        }

        return await query
            .OrderByDescending(asignacion => asignacion.FechaInicio)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AsignacionVehiculoConductor>> GetByConductorAsync(
        Guid empresaId,
        Guid conductorId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AsignacionesVehiculoConductor
            .AsNoTracking()
            .Include(asignacion => asignacion.Vehiculo)
            .Include(asignacion => asignacion.Conductor)
                .ThenInclude(conductor => conductor.Usuario)
            .Where(asignacion =>
                asignacion.EmpresaId == empresaId &&
                asignacion.ConductorId == conductorId)
            .OrderByDescending(asignacion => asignacion.FechaInicio)
            .ToListAsync(cancellationToken);
    }

    public async Task<FinalizarAsignacionResult> FinalizarAsync(
        Guid empresaId,
        Guid asignacionId,
        Guid? conductorId,
        CancellationToken cancellationToken = default)
    {
        var asignacion = await _context.AsignacionesVehiculoConductor.FirstOrDefaultAsync(
            item => item.EmpresaId == empresaId &&
                    item.Id == asignacionId &&
                    (!conductorId.HasValue || item.ConductorId == conductorId.Value),
            cancellationToken);
        if (asignacion is null)
        {
            return FinalizarAsignacionResult.NotFound;
        }

        if (asignacion.FechaFin.HasValue)
        {
            return FinalizarAsignacionResult.AlreadyFinished;
        }

        asignacion.FechaFin = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return FinalizarAsignacionResult.Success;
    }
}
