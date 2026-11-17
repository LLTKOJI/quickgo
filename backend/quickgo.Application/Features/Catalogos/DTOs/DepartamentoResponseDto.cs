namespace quickgo.Application.Features.Catalogos.DTOs;

public class DepartamentoResponseDto
{
    public Guid Id { get; set; }
    public Guid PaisId { get; set; }
    public string Nombre { get; set; } = string.Empty;
}