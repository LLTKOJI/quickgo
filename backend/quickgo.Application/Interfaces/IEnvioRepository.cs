using quickgo.Application.Features.Envios.DTOs;
using quickgo.Domain.entities;
using quickgo.Domain.Enums;

namespace quickgo.Application.Interfaces;

public interface IEnvioRepository
{
    Task<Cliente?> GetClienteAsync(Guid empresaId, Guid clienteId, CancellationToken ct = default);
    Task<Direccion?> GetDireccionAsync(Guid empresaId, Guid direccionId, CancellationToken ct = default);
    Task<Vehiculo?> GetVehiculoAsync(Guid empresaId, Guid vehiculoId, CancellationToken ct = default);
    Task<Conductor?> GetConductorAsync(Guid empresaId, Guid conductorId, CancellationToken ct = default);
    Task<bool> VehiculoTieneSeguroVigenteAsync(Guid empresaId, Guid vehiculoId, DateOnly hoy, CancellationToken ct = default);
    Task<bool> MonedaExisteAsync(string codigo, CancellationToken ct = default);

    Task<Envio> AddAsync(Envio envio, CancellationToken ct = default);
    Task<Envio?> GetByIdAsync(Guid empresaId, Guid envioId, CancellationToken ct = default);
    Task<List<Envio>> ListarAsync(Guid empresaId, EstadoEnvio? estado, DateOnly? desde, DateOnly? hasta, Guid? conductorId, CancellationToken ct = default);
    Task UpdateAsync(Envio envio, CancellationToken ct = default);

    Task<Cotizacion> AddCotizacionAsync(Cotizacion cotizacion, CancellationToken ct = default);
    Task<List<Cotizacion>> ListarCotizacionesAsync(Guid empresaId, Guid envioId, CancellationToken ct = default);
    Task<Cotizacion?> GetCotizacionAsync(Guid empresaId, Guid envioId, Guid cotizacionId, CancellationToken ct = default);
    Task AceptarCotizacionAsync(Envio envio, Cotizacion aceptada, List<Cotizacion> cotizaciones, CancellationToken ct = default);

    Task<SeguimientoEnvio> AddSeguimientoAsync(SeguimientoEnvio seguimiento, CancellationToken ct = default);
    Task<List<SeguimientoEnvio>> ListarSeguimientoAsync(Guid empresaId, Guid envioId, CancellationToken ct = default);
    Task<SeguimientoEnvio?> GetUltimoSeguimientoAsync(Guid empresaId, Guid envioId, CancellationToken ct = default);
    Task ActualizarEstadoVehiculoPorEnvioAsync(Envio envio, CancellationToken ct = default);
}
