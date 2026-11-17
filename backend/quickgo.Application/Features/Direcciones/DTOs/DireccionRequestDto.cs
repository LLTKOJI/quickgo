namespace quickgo.Application.Features.Direcciones.DTOs;

public class DireccionRequestDto {
    public Guid? ClienteId { get; set; }
    public Guid? CiudadId { get; set; }
    public string CallePrincipal { get; set; } = string.Empty;
    public string? NumeroExterior { get; set; }
    public string? ColoniaSector { get; set; }
    public string? Referencia { get; set; }
    public decimal? Lat { get; set; }
    public decimal? Lng { get; set; }
}