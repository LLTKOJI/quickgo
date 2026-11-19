namespace quickgo.Application.Features.Asignacion.DTOs;

public class AsignacionResponseDTO
{
    public Guid Id { get; set; }
    public Guid VehiculoId { get; set; }
    public string Placa { get; set; } = string.Empty;
    public Guid ConductorId { get; set; }
    public string NombreConductor { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public bool Vigente { get; set; }
}
