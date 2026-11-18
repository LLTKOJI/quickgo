using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using quickgo.Domain.Enums;

namespace quickgo.Application.Features.Vehiculo.DTOs;

public class CreateVehiculoRequestDTO
{
    [Required]
    public Guid TipoVehiculoId { get; set; }

    [Required]
    [StringLength(20)]
    public string Placa { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Vin { get; set; }

    [Required]
    [JsonRequired]
    public PropiedadVehiculo Propiedad { get; set; }
}
