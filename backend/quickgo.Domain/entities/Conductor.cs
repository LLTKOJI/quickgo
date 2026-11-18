using quickgo.Domain.enums;

namespace quickgo.Domain.entities;

public class Conductor : IEmpresaScoped {
    // Es PK y FK de usuarios.id
    public Guid Id { get; set; }
    public Usuario? Usuario { get; set; }

    public Guid EmpresaId { get; set; }
    public Empresa? Empresa { get; set; }

    public TipoConductor TipoConductor { get; set; }
    public string Licencia { get; set; } = string.Empty;
    public DateOnly VenceLicencia { get; set; }
    public EstadoConductor Estado { get; set; } = EstadoConductor.Activo;
}
