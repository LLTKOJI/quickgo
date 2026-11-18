namespace quickgo.Domain.entities;

public class Factura {
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public Guid EnvioId { get; set; }
    public decimal Monto { get; set; }
    public string MonedaCodigo { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; }
    public List<Pago> Pagos { get; set; } = new();
}