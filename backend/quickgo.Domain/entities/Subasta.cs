using quickgo.Domain.enums;

namespace quickgo.Domain.entities;

public class Subasta
{
    public int SubastaId { get; set; }

    // Empresa contratante dueña de la subasta
    public int EmpresaId { get; set; }
    public Empresa? Empresa { get; set; }

    public int TipoVehiculoId { get; set; }
    public TipoVehiculo? TipoVehiculo { get; set; }

    public int OrigenDireccionId { get; set; }
    public int DestinoDireccionId { get; set; }

    public string? Descripcion { get; set; }
    public decimal PesoKg { get; set; }
    public decimal? VolumenM3 { get; set; }
    public decimal PrecioMaximo { get; set; }
    public string MonedaCodigo { get; set; } = string.Empty;
    public DateTimeOffset FechaCierre { get; set; }
    public EstadoSubasta Estado { get; set; } = EstadoSubasta.Abierta;

    public ICollection<Puja> Pujas { get; set; } = new List<Puja>();
}