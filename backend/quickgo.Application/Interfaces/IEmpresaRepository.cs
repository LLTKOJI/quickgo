using quickgo.Domain.entities;

namespace quickgo.Application.Interfaces;

public interface IEmpresaRepository
{
    Task<IEnumerable<Empresa>> GetAllEmpresasAsync();
    Task<Empresa> AddEmpresaAsync(Empresa empresa);
    Task<Empresa?> UpdateEmpresaAsync(Empresa empresa);
    Task<bool> DeleteEmpresaAsync(Guid id);
    Task<Empresa?> GetByIdAsync(Guid id);
    Task<bool> ExisteRtnAsync(string rtn);
    Task<bool> ExisteSlugAsync(string slug);
}
