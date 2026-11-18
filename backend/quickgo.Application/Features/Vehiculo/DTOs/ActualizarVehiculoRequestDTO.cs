using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using quickgo.Domain.Enums;

namespace quickgo.Application.Features.Vehiculo.DTOs;

public class ActualizarVehiculoRequestDTO
{
    [Required]
    [JsonRequired]
    public EstadoVehiculo Estado { get; set; }

    [StringLength(50)]
    public string? Vin { get; set; }

    [StringLength(20)]
    public string? Placa { get; set; }
}
