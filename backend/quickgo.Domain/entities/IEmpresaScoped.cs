namespace quickgo.Domain.entities;

/// <summary>
/// Marca las entidades que pertenecen a una empresa. El DbContext les aplica
/// automaticamente el filtro global por empresa_id (multiempresa).
/// </summary>
public interface IEmpresaScoped {
    Guid EmpresaId { get; set; }
}
