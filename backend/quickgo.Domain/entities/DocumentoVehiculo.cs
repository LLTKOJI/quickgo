using quickgo.Domain.Enums;

namespace quickgo.Domain.entities;

public class DocumentoVehiculo
{
    public Guid Id { get; set; }
    public Guid VehiculoId { get; set; }
    public TipoDocumento Tipo { get; set; }
    public string Numero { get; set; } = string.Empty;
    public DateOnly FechaVencimiento { get; set; }
    public Vehiculo Vehiculo { get; set; } = null!;
}
