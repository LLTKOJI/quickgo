using Microsoft.EntityFrameworkCore;
using quickgo.Application.Interfaces;
using quickgo.Domain.Constants;
using quickgo.Domain.entities;
using quickgo.Infrastructure.Data;

namespace quickgo.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly QuickGoDbContext _context;

    public UsuarioRepository(QuickGoDbContext context)
    {
        _context = context;
    }

    // Login y correo unico: aun no hay empresa en el token, por eso se ignora el filtro global.
    public Task<Usuario?> GetParaLoginAsync(string email) =>
        _context.Usuarios.IgnoreQueryFilters()
            .Include(u => u.Rol)
            .Include(u => u.Empresa)
            .FirstOrDefaultAsync(u => u.Email == email);

    public Task<bool> ExisteEmailAsync(string email) =>
        _context.Usuarios.IgnoreQueryFilters().AnyAsync(u => u.Email == email);

    public Task<Usuario?> GetByIdAsync(Guid id) =>
        _context.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Id == id);

    public async Task<IReadOnlyList<Usuario>> ListarAsync() =>
        await _context.Usuarios.AsNoTracking()
            .Include(u => u.Rol)
            .OrderBy(u => u.PrimerNombre).ThenBy(u => u.PrimerApellido)
            .ToListAsync();

    public Task<int> ContarAdminsActivosAsync() =>
        _context.Usuarios.CountAsync(u => u.Activo && u.Rol!.Nombre == Roles.Admin);

    public void Add(Usuario usuario) => _context.Usuarios.Add(usuario);
}
