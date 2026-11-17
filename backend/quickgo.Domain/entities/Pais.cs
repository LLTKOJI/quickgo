namespace quickgo.Domain.entities;

public class Pais {
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string CodigoIso { get; set; } = string.Empty;
}
