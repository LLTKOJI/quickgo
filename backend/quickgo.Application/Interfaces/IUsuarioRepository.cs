using quickgo.Domain.entities;

namespace quickgo.Application.Interfaces;

public interface IUsuarioRepository {
    /// <summary>Busca por correo en todas las empresas (ignora el filtro). Incluye Rol y Empresa.</summary>
    Task<Usuario?> GetParaLoginAsync(string email);

    /// <summary>Correo unico en toda la plataforma.</summary>
    Task<bool> ExisteEmailAsync(string email);

    /// <summary>Usuario de la empresa actual (filtro global). Incluye Rol. Rastreado para editar.</summary>
    Task<Usuario?> GetByIdAsync(Guid id);

    Task<IReadOnlyList<Usuario>> ListarAsync();
    Task<int> ContarAdminsActivosAsync();
    void Add(Usuario usuario);
}
