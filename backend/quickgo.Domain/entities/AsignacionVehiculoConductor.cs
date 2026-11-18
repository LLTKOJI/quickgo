namespace quickgo.Domain.entities;

public class AsignacionVehiculoConductor
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public Guid VehiculoId { get; set; }
    public Guid ConductorId { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public Vehiculo Vehiculo { get; set; } = null!;
    public Conductor Conductor { get; set; } = null!;
}
