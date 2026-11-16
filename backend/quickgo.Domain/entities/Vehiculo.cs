namespace quickgo.Domain.entities;

public class Vehiculo
{
    public int VehiculoId { get; set; }
    public Empresa EmpresaId { get; set; }
    public TipoVehiculo TipoVehiculo { get; set; }
    public string Vin { get; set; } = string.Empty;
    public string Placa { get; set; } = string.Empty;
    public string Propiedad { get; set; } = string.Empty;
    public string Estado { get; set; } =  string.Empty;
}