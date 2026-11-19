namespace quickgo.Domain.entities;

public class SeguimientoEnvio : IEmpresaScoped
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public Guid EnvioId { get; set; }
    public decimal Lat { get; set; }
    public decimal Lng { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTimeOffset Momento { get; set; }
}
