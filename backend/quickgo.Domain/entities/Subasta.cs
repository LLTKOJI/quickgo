using quickgo.Domain.enums;

namespace quickgo.Domain.entities;

public class Subasta
{
    public Guid Id { get; set; }

    // Empresa contratante dueña de la subasta
    public Guid EmpresaId { get; set; }
    public Empresa? Empresa { get; set; }

    public Guid TipoVehiculoId { get; set; }
    public TipoVehiculo? TipoVehiculo { get; set; }

    public Guid OrigenDireccionId { get; set; }
    public Guid DestinoDireccionId { get; set; }
    public Direccion? OrigenDireccion { get; set; }
    public Direccion? DestinoDireccion { get; set; }

    public string? Descripcion { get; set; }
    public decimal PesoKg { get; set; }
    public decimal? VolumenM3 { get; set; }
    public decimal PrecioMaximo { get; set; }
    public string MonedaCodigo { get; set; } = string.Empty;
    public Moneda? Moneda { get; set; }
    public DateTimeOffset FechaCierre { get; set; }
    public EstadoSubasta Estado { get; set; } = EstadoSubasta.Abierta;

    public ICollection<Puja> Pujas { get; set; } = new List<Puja>();
}