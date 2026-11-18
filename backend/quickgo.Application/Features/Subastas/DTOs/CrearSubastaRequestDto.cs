namespace quickgo.Application.Features.Subastas.DTOs;

public class CrearSubastaRequestDto {
    public Guid TipoVehiculoId { get; set; }
    public Guid OrigenDireccionId { get; set; }
    public Guid DestinoDireccionId { get; set; }
    public string? Descripcion { get; set; }
    public decimal PesoKg { get; set; }
    public decimal? VolumenM3 { get; set; }
    public decimal PrecioMaximo { get; set; }
    public string MonedaCodigo { get; set; } = string.Empty;
    public DateTimeOffset FechaCierre { get; set; }
}