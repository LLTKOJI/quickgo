namespace quickgo.Domain.entities;

public class Envio {
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal Tarifa { get; set; }
}