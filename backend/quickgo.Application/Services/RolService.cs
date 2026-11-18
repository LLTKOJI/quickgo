using quickgo.Application.Features.Identidad.DTOs;
using quickgo.Application.Interfaces;

namespace quickgo.Application.Services;

/// <summary>B1-03: catalogo global de roles.</summary>
public class RolService {
    private readonly IRolRepository _roles;

    public RolService(IRolRepository roles) {
        _roles = roles;
    }

    public async Task<IReadOnlyList<RolResponse>> ListarAsync() {
        var roles = await _roles.GetAllAsync();
        return roles.Select(r => new RolResponse(r.Id, r.Nombre, r.Descripcion)).ToList();
    }
}
