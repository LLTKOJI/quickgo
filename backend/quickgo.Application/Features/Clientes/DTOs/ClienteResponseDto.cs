namespace quickgo.Application.Features.Clientes.DTOs;


public class ClienteResponseDto {
    public Guid Id { get; set; }
    public string PrimerNombre { get; set; } = string.Empty;
    public string? SegundoNombre { get; set; }
    public string PrimerApellido { get; set; } = string.Empty;
    public string? SegundoApellido { get; set; }
    public string Telefono { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string TipoCliente { get; set; } = string.Empty;
}
