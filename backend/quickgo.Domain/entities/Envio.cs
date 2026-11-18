using quickgo.Domain.Enums;

namespace quickgo.Domain.entities;

public class Envio
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public Guid VehiculoId { get; set; }
    public EstadoEnvio Estado { get; set; }
    public decimal Tarifa { get; set; }
    public Guid VehiculoId { get; set; }
}
