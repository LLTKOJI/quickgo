using Microsoft.EntityFrameworkCore;
using quickgo.Application.Interfaces;
using quickgo.Domain.entities;
using quickgo.Infrastructure.Data;

namespace quickgo.Infrastructure.Repositories;

public class EmpresaRepository : IEmpresaRepository
{
    private readonly QuickGoDbContext _context;

    public EmpresaRepository(QuickGoDbContext context)
    {
        _context = context;
    }

    public Task<Empresa?> GetByIdAsync(Guid id) =>
        _context.Empresas.FirstOrDefaultAsync(e => e.Id == id);

    public Task<bool> ExisteRtnAsync(string rtn) =>
        _context.Empresas.AnyAsync(e => e.Rtn == rtn);

    public Task<bool> ExisteSlugAsync(string slug) =>
        _context.Empresas.AnyAsync(e => e.Slug == slug);

    public void Add(Empresa empresa) => _context.Empresas.Add(empresa);
}
