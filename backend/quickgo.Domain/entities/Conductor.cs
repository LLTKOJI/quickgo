namespace quickgo.Domain.entities;

public class Conductor
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public Guid UsuarioId { get; set; }
    public bool Activo { get; set; } = true;
    public Usuario Usuario { get; set; } = null!;
    public ICollection<AsignacionVehiculoConductor> Asignaciones { get; set; } =
        new List<AsignacionVehiculoConductor>();
}
