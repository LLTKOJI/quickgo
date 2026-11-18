using Microsoft.EntityFrameworkCore;
using Npgsql;
using quickgo.Application.Common;
using quickgo.Application.Interfaces;
using quickgo.Infrastructure.Data;

namespace quickgo.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly QuickGoDbContext _context;

    public UnitOfWork(QuickGoDbContext context)
    {
        _context = context;
    }

    public async Task GuardarAsync(CancellationToken ct = default)
    {
        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } pg)
        {
            throw new ConflictoException(pg.ConstraintName);
        }
    }
}
