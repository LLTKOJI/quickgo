namespace quickgo.Domain.entities;

public class Usuario
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public Conductor? Conductor { get; set; }
}
