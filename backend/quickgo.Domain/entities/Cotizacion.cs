namespace quickgo.Domain.entities;

public class Cotizacion {
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public Guid EnvioId { get; set; }
    public decimal Monto { get; set; }
    public string MonedaCodigo { get; set; } = string.Empty;
    public DateTime ValidaHasta { get; set; }
    public bool Aceptada { get; set; }
}
