namespace quickgo.Application.Features.Direcciones.DTOs;

public class DireccionResponseDto {
    public Guid Id { get; set; }
    public Guid? ClienteId { get; set; }
    public Guid CiudadId { get; set; }
    public string CiudadNombre { get; set; } = string.Empty;
    public string CallePrincipal { get; set; } = string.Empty;
    public string? NumeroExterior { get; set; }
    public string? ColoniaSector { get; set; }
    public string? Referencia { get; set; }
    public decimal Lat { get; set; }
    public decimal Lng { get; set; }
}
