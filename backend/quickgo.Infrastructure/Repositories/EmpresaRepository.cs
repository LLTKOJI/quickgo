using quickgo.Application.Interfaces;
using quickgo.Domain.entities;
using quickgo.Infrastructure.Data;

namespace quickgo.Infrastructure.Repositories;

public class EmpresaRepository : IEmpresaRepository
{
    private readonly QuickGoDbContext  _context;

    public EmpresaRepository(QuickGoDbContext context)
    {
        _context = context;
    }

    public Task<IEnumerable<Empresa>> GetAllEmpresasAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Empresa> GetEmpresaAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<Empresa> AddEmpresaAsync(Empresa empresa)
    {
        throw new NotImplementedException();
    }

    public Task<Empresa> UpdateEmpresaAsync(Empresa empresa)
    {
        throw new NotImplementedException();
    }

    public Task DeleteEmpresaAsync(int id)
    {
        throw new NotImplementedException();
    }
}