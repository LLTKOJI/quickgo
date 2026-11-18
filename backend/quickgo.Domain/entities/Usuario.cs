namespace quickgo.Domain.entities;

public class Usuario : IEmpresaScoped {
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public Empresa? Empresa { get; set; }
    public Guid RolId { get; set; }
    public Rol? Rol { get; set; }

    public string PrimerNombre { get; set; } = string.Empty;
    public string? SegundoNombre { get; set; }
    public string PrimerApellido { get; set; } = string.Empty;
    public string? SegundoApellido { get; set; }
    public string? Telefono { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;

    // Solo lectura: no se mapea a la base (no tiene setter).
    public string NombreCompleto => string.Join(' ',
        new[] { PrimerNombre, SegundoNombre, PrimerApellido, SegundoApellido }
            .Where(p => !string.IsNullOrWhiteSpace(p)));
}
