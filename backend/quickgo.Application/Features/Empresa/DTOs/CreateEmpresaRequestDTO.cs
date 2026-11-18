using System.ComponentModel.DataAnnotations;

namespace quickgo.Application.Features.Empresa.DTOs;

public class CreateEmpresaRequestDTO
{
    [Required]
    public string NombreComercial { get; set; } = string.Empty;

    [Required]
    public string RazonSocial { get; set; } = string.Empty;

    [Required]
    public string RTN { get; set; } = string.Empty;

    [Required]
    public string TipoComercial { get; set; } = string.Empty;

    [Required]
    public string Plan { get; set; } = string.Empty;

    public bool Activa { get; set; }
}