namespace quickgo.Application.Features.Catalogos.DTOs;

public class PaisResponseDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string CodigoIso { get; set; } = string.Empty;
}
