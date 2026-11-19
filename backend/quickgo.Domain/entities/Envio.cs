using quickgo.Domain.Enums;

namespace quickgo.Domain.entities;

public class Envio : IEmpresaScoped
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public Guid ClienteId { get; set; }
    public Guid? SubastaId { get; set; }
    public Guid OrigenDireccionId { get; set; }
    public Guid DestinoDireccionId { get; set; }
    public Guid? VehiculoId { get; set; }
    public Guid? ConductorId { get; set; }
    public TipoServicio? TipoServicio { get; set; }
    public EstadoEnvio Estado { get; set; } = EstadoEnvio.Pendiente;
    public decimal? PesoKg { get; set; }
    public decimal? VolumenM3 { get; set; }
    public DateTimeOffset FechaProgramada { get; set; }
    public decimal Tarifa { get; set; }
    public decimal CostoExterno { get; set; }
}
