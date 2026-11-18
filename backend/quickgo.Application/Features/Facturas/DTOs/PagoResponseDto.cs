namespace quickgo.Application.Features.Facturas.DTOs;

public class PagoResponseDto {
    public Guid Id { get; set; }
    public string Metodo { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public DateTime FechaPago { get; set; }
}