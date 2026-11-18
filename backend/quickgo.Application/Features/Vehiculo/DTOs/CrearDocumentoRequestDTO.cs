using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using quickgo.Domain.Enums;

namespace quickgo.Application.Features.Vehiculo.DTOs;

public class CrearDocumentoRequestDTO
{
    [Required]
    [JsonRequired]
    public TipoDocumento Tipo { get; set; }

    [Required]
    [StringLength(80)]
    public string Numero { get; set; } = string.Empty;

    [Required]
    public DateOnly FechaVencimiento { get; set; }
}
