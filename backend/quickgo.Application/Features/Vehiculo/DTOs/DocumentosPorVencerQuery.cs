using System.ComponentModel.DataAnnotations;

namespace quickgo.Application.Features.Vehiculo.DTOs;

public class DocumentosPorVencerQuery
{
    [Range(1, 365)]
    public int Dias { get; set; } = 30;
}
