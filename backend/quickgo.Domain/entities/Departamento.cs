namespace quickgo.Domain.entities;

public class Departamento {
    public Guid Id { get; set; }
    public Guid PaisId { get; set; }
    public string Nombre { get; set; } = string.Empty;
}