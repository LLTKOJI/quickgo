using System.ComponentModel.DataAnnotations;

namespace quickgo.Application.Features.Asignacion.DTOs;

public class CrearAsignacionRequestDTO
{
    [Required]
    public Guid VehiculoId { get; set; }

    [Required]
    public Guid ConductorId { get; set; }
}
