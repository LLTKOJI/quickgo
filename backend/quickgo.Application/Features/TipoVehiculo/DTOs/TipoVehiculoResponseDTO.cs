namespace quickgo.Application.Features.TipoVehiculo.DTOs;

public class TipoVehiculoResponseDTO
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal CapacidadKg { get; set; }
    public decimal LargoM { get; set; }
    public decimal AnchoM { get; set; }
    public decimal AltoM { get; set; }
    public decimal VolumenMaxM3 { get; set; }
}
