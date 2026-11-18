using quickgo.Domain.enums;

namespace quickgo.Application.Features.Subastas.DTOs;

public class SubastaResponseDto {
    public Guid Id { get; set; }
    public Guid TipoVehiculoId { get; set; }
    public string TipoVehiculo { get; set; } = string.Empty;
    public Guid OrigenDireccionId { get; set; }
    public Guid DestinoDireccionId { get; set; }
    public string? Descripcion { get; set; }
    public decimal PesoKg { get; set; }
    public decimal? VolumenM3 { get; set; }
    public decimal PrecioMaximo { get; set; }
    public string MonedaCodigo { get; set; } = string.Empty;
    public DateTimeOffset FechaCierre { get; set; }
    public EstadoSubasta Estado { get; set; }
    public int TotalPujas { get; set; }
    public Guid? EnvioId { get; set; }
}