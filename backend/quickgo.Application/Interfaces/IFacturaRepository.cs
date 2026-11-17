using quickgo.Domain.entities;
namespace quickgo.Application.Interfaces;

public interface IFacturaRepository {
    Task<IEnumerable<Factura>> GetAllAsync(Guid empresaId, string? estado);
    Task<Factura?> GetByIdAsync(Guid empresaId, Guid id);
    Task<Envio?> GetEnvioAsync(Guid empresaId, Guid envioId);
    Task<bool> ExisteFacturaDeEnvioAsync(Guid envioId);
    Task<string?> GetMonedaCotizacionAceptadaAsync(Guid envioId);
    Task<bool> ExisteMonedaAsync(string codigo);
    Task<Factura> AddAsync(Factura factura);
    Task AgregarPagoAsync(Factura factura, Pago pago);
}
