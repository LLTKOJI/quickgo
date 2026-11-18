using quickgo.Domain.entities;

namespace quickgo.Application.Interfaces;

public interface IEmpresaRepository
{
    Task<Empresa?> GetByIdAsync(Guid id);
    Task<bool> ExisteRtnAsync(string rtn);
    Task<bool> ExisteSlugAsync(string slug);
    void Add(Empresa empresa);
}
