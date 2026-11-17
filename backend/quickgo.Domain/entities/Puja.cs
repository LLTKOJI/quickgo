using quickgo.Domain.enums;

namespace quickgo.Domain.entities;

public class Puja
{
    public int PujaId { get; set; }

    public int SubastaId { get; set; }
    public Subasta? Subasta { get; set; }

    // Empresa transportista que puja
    public int EmpresaId { get; set; }
    public Empresa? Empresa { get; set; }

    public int ConductorId { get; set; }

    public decimal Monto { get; set; }
    public string? Mensaje { get; set; }
    public EstadoPuja Estado { get; set; } = EstadoPuja.Activa;
}