using quickgo.Domain.enums;

namespace quickgo.Domain.entities;

public class Puja
{
    public Guid Id { get; set; }

    public Guid SubastaId { get; set; }
    public Subasta? Subasta { get; set; }

    // Empresa transportista que puja
    public Guid EmpresaId { get; set; }
    public Empresa? Empresa { get; set; }

    // Todavía no existe la entidad Conductor; se agrega la navegación cuando se suba
    public Guid ConductorId { get; set; }

    public decimal Monto { get; set; }
    public string? Mensaje { get; set; }
    public EstadoPuja Estado { get; set; } = EstadoPuja.Activa;
}