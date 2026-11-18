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

    public async Task<IEnumerable<Empresa>> GetAllEmpresasAsync()
    {
        return await _context.Empresas.AsNoTracking().ToListAsync();
    }

    public async Task<Empresa> AddEmpresaAsync(Empresa empresa)
    {
        await _context.Empresas.AddAsync(empresa);
        await _context.SaveChangesAsync();
        return empresa;
    }

    public async Task<Empresa?> UpdateEmpresaAsync(Empresa empresa)
    {
        var existingEmpresa = await _context.Empresas.FindAsync(empresa.EmpresaId);
        if (existingEmpresa is null)
        {
            return null;
        }

        existingEmpresa.NombreComercial = empresa.NombreComercial;
        existingEmpresa.RazonSocial = empresa.RazonSocial;
        existingEmpresa.RTN = empresa.RTN;
        existingEmpresa.TipoComercial = empresa.TipoComercial;
        existingEmpresa.Plan = empresa.Plan;
        existingEmpresa.Activa = empresa.Activa;

        await _context.SaveChangesAsync();
        return existingEmpresa;
    }

    public async Task<bool> DeleteEmpresaAsync(Guid id)
    {
        var empresa = await _context.Empresas.FindAsync(id);
        if (empresa is null)
        {
            return false;
        }

        _context.Empresas.Remove(empresa);
        await _context.SaveChangesAsync();
        return true;
    }

    public Task<Empresa?> GetByIdAsync(Guid id) =>
        _context.Empresas.FirstOrDefaultAsync(e => e.Id == id);

    public Task<bool> ExisteRtnAsync(string rtn) =>
        _context.Empresas.AnyAsync(e => e.Rtn == rtn);

    public Task<bool> ExisteSlugAsync(string slug) =>
        _context.Empresas.AnyAsync(e => e.Slug == slug);
}
