namespace quickgo.Application.Features.Facturas.DTOs;

public class CreatePagoRequestDto {
    public string Metodo { get; set; } = string.Empty;
    public decimal Monto { get; set; }
}
