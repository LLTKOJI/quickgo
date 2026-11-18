using quickgo.Domain.Enums;

namespace quickgo.Application.Features.Vehiculo.DTOs;

public class DocumentoResponseDTO
{
    public Guid Id { get; set; }
    public Guid VehiculoId { get; set; }
    public TipoDocumento Tipo { get; set; }
    public string Numero { get; set; } = string.Empty;
    public DateOnly FechaVencimiento { get; set; }
    public bool Vencido { get; set; }
}
