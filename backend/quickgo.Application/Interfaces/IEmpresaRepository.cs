using quickgo.Domain.entities;

namespace quickgo.Application.Interfaces;

public interface IEmpresaRepository
{
    Task<IEnumerable<Empresa>> GetAllEmpresasAsync();
    Task<Empresa> GetEmpresaAsync(int id);
    Task<Empresa>AddEmpresaAsync(Empresa empresa);
    Task<Empresa> UpdateEmpresaAsync(Empresa empresa);
    Task DeleteEmpresaAsync(int id);
    
}