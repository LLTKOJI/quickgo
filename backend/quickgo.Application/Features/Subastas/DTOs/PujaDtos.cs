using quickgo.Domain.enums;

namespace quickgo.Application.Features.Subastas.DTOs;

public class SubastasAbiertasQuery
{
    public Guid? TipoVehiculoId { get; set; }
}

public class SubastaAbiertaResponse
{
    public Guid Id { get; set; }
    public Guid TipoVehiculoId { get; set; }
    public string TipoVehiculo { get; set; } = string.Empty;
    public string OrigenCiudad { get; set; } = string.Empty;
    public string DestinoCiudad { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal PesoKg { get; set; }
    public decimal? VolumenM3 { get; set; }
    public decimal PrecioMaximo { get; set; }
    public string MonedaCodigo { get; set; } = string.Empty;
    public DateTimeOffset FechaCierre { get; set; }
    public decimal? MejorOferta { get; set; }
    public bool YaPuje { get; set; }
}

public class CrearPujaRequest
{
    public Guid ConductorId { get; set; }
    public decimal Monto { get; set; }
    public string? Mensaje { get; set; }
}

public class PujaResponse
{
    public Guid Id { get; set; }
    public Guid SubastaId { get; set; }
    public Guid ConductorId { get; set; }
    public decimal Monto { get; set; }
    public string? Mensaje { get; set; }
    public EstadoPuja Estado { get; set; }
}

public class MejorOfertaResponse
{
    public Guid SubastaId { get; set; }
    public decimal? Monto { get; set; }
    public string MonedaCodigo { get; set; } = string.Empty;
    public bool EsMia { get; set; }
}

public class PujaDetalleResponse
{
    public Guid Id { get; set; }
    public Guid SubastaId { get; set; }
    public Guid EmpresaId { get; set; }
    public string EmpresaNombre { get; set; } = string.Empty;
    public Guid ConductorId { get; set; }
    public string ConductorNombre { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string? Mensaje { get; set; }
    public EstadoPuja Estado { get; set; }
}

public class AdjudicarRequest
{
    public Guid PujaId { get; set; }
}
