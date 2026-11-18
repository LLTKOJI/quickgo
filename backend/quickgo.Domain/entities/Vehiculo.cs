using quickgo.Domain.Enums;

namespace quickgo.Domain.entities;

public class Vehiculo : IEmpresaScoped
{
    public Guid VehiculoId { get; set; }
    public Guid EmpresaId { get; set; }
    public Guid TipoVehiculoId { get; set; }
    public string? Vin { get; set; }
    public string Placa { get; set; } = string.Empty;
    public PropiedadVehiculo Propiedad { get; set; }
    public EstadoVehiculo Estado { get; set; }
    public TipoVehiculo TipoVehiculo { get; set; } = null!;
    public ICollection<DocumentoVehiculo> Documentos { get; set; } = new List<DocumentoVehiculo>();
    public ICollection<AsignacionVehiculoConductor> Asignaciones { get; set; } =
        new List<AsignacionVehiculoConductor>();
    public ICollection<Envio> Envios { get; set; } = new List<Envio>();
}
