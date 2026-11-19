using quickgo.Domain.enums;

namespace quickgo.Domain.entities;

public class Empresa
{
    public Guid Id { get; set; }
    public string NombreComercial { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Rtn { get; set; } = string.Empty;
    public TipoEmpresa Tipo { get; set; }
    public PlanEmpresa? Plan { get; set; }
    public bool Activa { get; set; } = true;
}
