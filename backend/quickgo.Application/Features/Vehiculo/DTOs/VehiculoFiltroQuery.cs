using quickgo.Domain.Enums;

namespace quickgo.Application.Features.Vehiculo.DTOs;

public class VehiculoFiltroQuery
{
    public EstadoVehiculo? Estado { get; set; }
    public Guid? TipoVehiculoId { get; set; }
    public bool IncluirBaja { get; set; }
}
