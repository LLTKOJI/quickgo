namespace quickgo.Domain.entities;

public class TipoVehiculo
{
    public Guid TipoVehiculoId { get; set; }
    public string Nombre {get; set;} = string.Empty;
    public decimal Cantidadkilogramo { get; set; }
    public decimal LargoMetros { get; set; }
    public decimal AnchoMetros { get; set; }
    public decimal AlturaMetros { get; set; }
    public decimal VolumenMaximoMetrosCubicos { get; set; }
    public ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();
}