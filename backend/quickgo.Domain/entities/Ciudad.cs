namespace quickgo.Domain.entities;

public class Ciudad {
    public Guid Id { get; set; }
    public Guid DepartamentoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? CodigoPostal { get; set; }
}
