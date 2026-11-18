namespace quickgo.Application.Features.Facturas.DTOs;

public class FacturaDetalleResponseDto : FacturaResponseDto {
    public List<PagoResponseDto> Pagos { get; set; } = new();
}
