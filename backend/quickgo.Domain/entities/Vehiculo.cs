namespace quickgo.Domain.entities;

public class Vehiculo : IEmpresaScoped
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public Empresa? Empresa { get; set; }
    public Guid TipoVehiculoId { get; set; }
    public TipoVehiculo? TipoVehiculo { get; set; }
    public string? Vin { get; set; }
    public string Placa { get; set; } = string.Empty;
    public string Propiedad { get; set; } = string.Empty;
    public string Estado { get; set; } =  string.Empty;
}
