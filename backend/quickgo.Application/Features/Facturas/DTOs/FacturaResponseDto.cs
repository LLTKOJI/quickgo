namespace quickgo.Application.Features.Facturas.DTOs;

public class FacturaResponseDto {
    public Guid Id { get; set; }
    public Guid EnvioId { get; set; }
    public decimal Monto { get; set; }
    public string MonedaCodigo { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; }
    public decimal TotalPagado { get; set; }
    public decimal SaldoPendiente { get; set; }
}