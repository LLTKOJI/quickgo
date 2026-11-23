using quickgo.Domain.Enums;

namespace quickgo.Application.Features.Envios.DTOs;

public class DireccionResumenResponse
{
    public Guid Id { get; set; }
    public string Texto { get; set; } = string.Empty;
    public decimal Lat { get; set; }
    public decimal Lng { get; set; }
}

public class CrearEnvioRequest
{
    public Guid ClienteId { get; set; }
    public Guid OrigenDireccionId { get; set; }
    public Guid DestinoDireccionId { get; set; }
    public TipoServicio TipoServicio { get; set; }
    public DateTimeOffset FechaProgramada { get; set; }
    public decimal PesoKg { get; set; }
    public decimal? VolumenM3 { get; set; }
    public decimal Tarifa { get; set; }
}

public class ActualizarEnvioRequest : CrearEnvioRequest
{
}

public class EnvioFiltroQuery
{
    public EstadoEnvio? Estado { get; set; }
    public DateOnly? Desde { get; set; }
    public DateOnly? Hasta { get; set; }
}

public class EnvioResponse
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public Guid? SubastaId { get; set; }
    public Guid OrigenDireccionId { get; set; }
    public Guid DestinoDireccionId { get; set; }
    public Guid? VehiculoId { get; set; }
    public Guid? ConductorId { get; set; }
    public TipoServicio? TipoServicio { get; set; }
    public EstadoEnvio Estado { get; set; }
    public decimal? PesoKg { get; set; }
    public decimal? VolumenM3 { get; set; }
    public DateTimeOffset FechaProgramada { get; set; }
    public decimal Tarifa { get; set; }
    public decimal CostoExterno { get; set; }
    public decimal Margen => Tarifa - CostoExterno;
}

public class EnvioDetalleResponse : EnvioResponse
{
    public DireccionResumenResponse Origen { get; set; } = new();
    public DireccionResumenResponse Destino { get; set; } = new();
    public string? VehiculoPlaca { get; set; }
    public string? ConductorNombre { get; set; }
}

public class AsignarEnvioRequest
{
    public Guid VehiculoId { get; set; }
    public Guid ConductorId { get; set; }
    public decimal? CostoExterno { get; set; }
}

public class CambiarEstadoRequest
{
    public EstadoEnvio Estado { get; set; }
}

public class CrearCotizacionRequest
{
    public decimal Monto { get; set; }
    public string MonedaCodigo { get; set; } = string.Empty;
    public DateTimeOffset ValidaHasta { get; set; }
}

public class CotizacionResponse
{
    public Guid Id { get; set; }
    public Guid EnvioId { get; set; }
    public decimal Monto { get; set; }
    public string MonedaCodigo { get; set; } = string.Empty;
    public DateTimeOffset ValidaHasta { get; set; }
    public bool Aceptada { get; set; }
    public bool Vencida { get; set; }
}

public class RegistrarSeguimientoRequest
{
    public decimal Lat { get; set; }
    public decimal Lng { get; set; }
    public string Estado { get; set; } = string.Empty;
}

public class SeguimientoResponse
{
    public Guid Id { get; set; }
    public Guid EnvioId { get; set; }
    public decimal? Lat { get; set; }
    public decimal? Lng { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTimeOffset Momento { get; set; }
}
