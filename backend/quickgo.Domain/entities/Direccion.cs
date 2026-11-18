namespace quickgo.Domain.entities;

public class Direccion {
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public Guid? ClienteId { get; set; }
    public Guid CiudadId { get; set; }
    public string CallePrincipal { get; set; } = string.Empty;
    public string? NumeroExterior { get; set; }
    public string? ColoniaSector { get; set; }
    public string? Referencia { get; set; }
    public decimal Lat { get; set; }
    public decimal Lng { get; set; }
    public Ciudad? Ciudad { get; set; }
}