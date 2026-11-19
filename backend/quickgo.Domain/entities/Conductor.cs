using quickgo.Domain.enums;

namespace quickgo.Domain.entities;

/// <summary>Su Id es el mismo del usuario (PK y FK a usuarios.id).</summary>
public class Conductor : IEmpresaScoped
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public Empresa? Empresa { get; set; }
    public Usuario? Usuario { get; set; }
    public TipoConductor TipoConductor { get; set; }
    public string Licencia { get; set; } = string.Empty;
    public DateOnly VenceLicencia { get; set; }
    public EstadoConductor Estado { get; set; } = EstadoConductor.Activo;
    public ICollection<AsignacionVehiculoConductor> Asignaciones { get; set; } =
        new List<AsignacionVehiculoConductor>();
}
