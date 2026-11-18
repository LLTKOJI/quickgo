using quickgo.Domain.Enums;

namespace quickgo.Application.Features.Vehiculo.DTOs;

public class DocumentoPorVencerResponseDTO
{
    public Guid DocumentoId { get; set; }
    public Guid VehiculoId { get; set; }
    public string Placa { get; set; } = string.Empty;
    public TipoDocumento Tipo { get; set; }
    public string Numero { get; set; } = string.Empty;
    public DateOnly FechaVencimiento { get; set; }
    public int DiasRestantes { get; set; }
    public bool Vencido { get; set; }
}
