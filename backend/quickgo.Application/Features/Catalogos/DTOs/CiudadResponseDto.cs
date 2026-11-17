namespace quickgo.Application.Features.Catalogos.DTOs;

public class CiudadResponseDto
{
    public Guid Id { get; set; }
    public Guid DepartamentoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? CodigoPostal { get; set; }
}
