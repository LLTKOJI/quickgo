using quickgo.Domain.Enums;

namespace quickgo.Application.Features.Vehiculo.DTOs;

public class VehiculoResponseDTO
{
    public Guid Id { get; set; }
    public Guid TipoVehiculoId { get; set; }
    public string TipoVehiculo { get; set; } = string.Empty;
    public string Placa { get; set; } = string.Empty;
    public string? Vin { get; set; }
    public PropiedadVehiculo Propiedad { get; set; }
    public EstadoVehiculo Estado { get; set; }
    public bool SeguroVigente { get; set; }
}
