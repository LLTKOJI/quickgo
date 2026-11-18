namespace quickgo.Domain.entities;

public class Pago {
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public Guid FacturaId { get; set; }
    public string Metodo { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public DateTime FechaPago { get; set; }
}