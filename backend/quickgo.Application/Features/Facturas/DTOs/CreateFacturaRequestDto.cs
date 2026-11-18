namespace quickgo.Application.Features.Facturas.DTOs;

public class CreateFacturaRequestDto {
    public Guid? EnvioId { get; set; }
    public string? MonedaCodigo { get; set; }
}