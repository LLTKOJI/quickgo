namespace quickgo.Application.Features.Facturas.DTOs;

public class RegistrarPagoResponseDto : PagoResponseDto {
    public Guid FacturaId { get; set; }
    public decimal SaldoRestante { get; set; }
    public string EstadoFactura { get; set; } = string.Empty;
}
