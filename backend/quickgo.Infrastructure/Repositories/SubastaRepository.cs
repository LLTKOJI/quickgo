using Microsoft.EntityFrameworkCore;
using quickgo.Application.Interfaces;
using quickgo.Domain.entities;
using quickgo.Domain.Enums;
using quickgo.Domain.enums;
using quickgo.Infrastructure.Data;

namespace quickgo.Infrastructure.Repositories;

public class SubastaRepository : ISubastaRepository {
    private readonly QuickGoDbContext _context;

    public SubastaRepository(QuickGoDbContext context) {
        _context = context;
    }

    public async Task<bool> TipoVehiculoExisteAsync(Guid tipoVehiculoId) {
        // TEMPORAL: volver a "t.Id == tipoVehiculoId" cuando TipoVehiculo use Guid e Id
        return await _context.TipoVehiculos.AnyAsync(t => t.TipoVehiculoId.ToString() == tipoVehiculoId.ToString());
    }

    public async Task<bool> DireccionesSonDeEmpresaAsync(Guid empresaId, Guid origenId, Guid destinoId) {
        var ids = new[] { origenId, destinoId };
        var cantidad = await _context.Direcciones
            .Where(d => d.EmpresaId == empresaId && ids.Contains(d.Id))
            .CountAsync();
        return cantidad == 2;
    }

    public async Task<bool> MonedaExisteAsync(string codigo) {
        return await _context.Monedas.AnyAsync(m => m.Codigo == codigo);
    }

    public async Task<Subasta> AddAsync(Subasta subasta) {
        _context.Subastas.Add(subasta);
        await _context.SaveChangesAsync();
        return subasta;
    }

    public async Task<Subasta?> GetByIdAsync(Guid empresaId, Guid id) {
        return await _context.Subastas
            .Include(s => s.TipoVehiculo)
            .FirstOrDefaultAsync(s => s.Id == id && s.EmpresaId == empresaId);
    }

    public async Task<int> ContarPujasAsync(Guid subastaId) {
        return await _context.Pujas
            .IgnoreQueryFilters()
            .CountAsync(p => p.SubastaId == subastaId);
    }

    public async Task CancelarAsync(Subasta subasta) {
        var pujasActivas = await _context.Pujas
            .IgnoreQueryFilters()
            .Where(p => p.SubastaId == subasta.Id && p.Estado == EstadoPuja.Activa)
            .ToListAsync();

        foreach (var puja in pujasActivas) {
            puja.Estado = EstadoPuja.Retirada;
        }

        subasta.Estado = EstadoSubasta.Cancelada;
        await _context.SaveChangesAsync();
    }

    public async Task<List<Subasta>> ListarPorEmpresaAsync(Guid empresaId, EstadoSubasta? estado) {
        var query = _context.Subastas
            .Include(s => s.TipoVehiculo)
            .Where(s => s.EmpresaId == empresaId);

        if (estado.HasValue) {
            query = query.Where(s => s.Estado == estado.Value);
        }

        return await query.OrderByDescending(s => s.FechaCierre).ToListAsync();
    }

    public async Task<Dictionary<Guid, int>> ContarPujasPorSubastaAsync(IEnumerable<Guid> subastaIds) {
        var ids = subastaIds.ToList();
        return await _context.Pujas
            .IgnoreQueryFilters()
            .Where(p => ids.Contains(p.SubastaId))
            .GroupBy(p => p.SubastaId)
            .Select(g => new { SubastaId = g.Key, Total = g.Count() })
            .ToDictionaryAsync(x => x.SubastaId, x => x.Total);
    }

    public async Task<List<Subasta>> ObtenerAbiertasVencidasAsync(Guid empresaId) {
        var ahora = DateTimeOffset.UtcNow;
        return await _context.Subastas
            .Where(s => s.EmpresaId == empresaId && s.Estado == EstadoSubasta.Abierta && s.FechaCierre <= ahora)
            .ToListAsync();
    }

    public async Task<List<Subasta>> ObtenerAbiertasVencidasGlobalAsync() {
        var ahora = DateTimeOffset.UtcNow;
        return await _context.Subastas.IgnoreQueryFilters()
            .Where(s => s.Estado == EstadoSubasta.Abierta && s.FechaCierre <= ahora)
            .ToListAsync();
    }

    public async Task VencerAsync(IEnumerable<Subasta> subastas) {
        var lista = subastas.ToList();
        if (lista.Count == 0) return;
        var ids = lista.Select(s => s.Id).ToList();
        var pujasActivas = await _context.Pujas.IgnoreQueryFilters()
            .Where(p => ids.Contains(p.SubastaId) && p.Estado == EstadoPuja.Activa).ToListAsync();
        foreach (var puja in pujasActivas) puja.Estado = EstadoPuja.Perdedora;
        foreach (var subasta in lista) subasta.Estado = EstadoSubasta.Vencida;
        await _context.SaveChangesAsync();
    }

    public async Task<List<Subasta>> ListarAbiertasParaEmpresaAsync(Guid empresaId, Guid? tipoVehiculoId) {
        var ahora = DateTimeOffset.UtcNow;
        var tipos = _context.Vehiculos.Where(v => v.EmpresaId == empresaId && v.Estado != EstadoVehiculo.Baja)
            .Select(v => v.TipoVehiculoId).Distinct();
        var q = _context.Subastas.Include(s => s.TipoVehiculo)
            .Include(s => s.OrigenDireccion).ThenInclude(d => d!.Ciudad)
            .Include(s => s.DestinoDireccion).ThenInclude(d => d!.Ciudad)
            .Where(s => s.Estado == EstadoSubasta.Abierta && s.FechaCierre > ahora && s.EmpresaId != empresaId && tipos.Contains(s.TipoVehiculoId));
        if (tipoVehiculoId.HasValue) q = q.Where(s => s.TipoVehiculoId == tipoVehiculoId.Value);
        return await q.OrderBy(s => s.FechaCierre).ToListAsync();
    }

    public async Task<Dictionary<Guid, decimal>> ObtenerMejoresOfertasAsync(IEnumerable<Guid> subastaIds) {
        var ids = subastaIds.ToList();
        return await _context.Pujas.IgnoreQueryFilters()
            .Where(p => ids.Contains(p.SubastaId) && p.Estado == EstadoPuja.Activa)
            .GroupBy(p => p.SubastaId)
            .Select(g => new { SubastaId = g.Key, Monto = g.Min(x => x.Monto) })
            .ToDictionaryAsync(x => x.SubastaId, x => x.Monto);
    }

    public async Task<HashSet<Guid>> ObtenerSubastasPujadasPorEmpresaAsync(Guid empresaId, IEnumerable<Guid> subastaIds) {
        var ids = subastaIds.ToList();
        var pujadas = await _context.Pujas.IgnoreQueryFilters()
            .Where(p => p.EmpresaId == empresaId && ids.Contains(p.SubastaId) && p.Estado != EstadoPuja.Retirada)
            .Select(p => p.SubastaId).Distinct().ToListAsync();
        return pujadas.ToHashSet();
    }

    public Task<Subasta?> GetAbiertaByIdAsync(Guid id) =>
        _context.Subastas.Include(s => s.TipoVehiculo).FirstOrDefaultAsync(s => s.Id == id);

    public async Task<bool> EmpresaTieneVehiculoElegibleAsync(Guid empresaId, Guid tipoVehiculoId, DateOnly hoy) {
        return await _context.Vehiculos.Where(v => v.EmpresaId == empresaId && v.TipoVehiculoId == tipoVehiculoId && v.Estado != EstadoVehiculo.Baja)
            .AnyAsync(v => _context.DocumentosVehiculo.Any(d => d.EmpresaId == empresaId && d.VehiculoId == v.VehiculoId && d.Tipo == TipoDocumento.Seguro && d.FechaVencimiento >= hoy));
    }

    public Task<Conductor?> GetConductorActivoDeEmpresaAsync(Guid empresaId, Guid conductorId) =>
        _context.Conductores.Include(c => c.Usuario).FirstOrDefaultAsync(c => c.EmpresaId == empresaId && c.Id == conductorId && c.Estado == quickgo.Domain.enums.EstadoConductor.Activo);

    public Task<Puja?> GetPujaEmpresaAsync(Guid subastaId, Guid empresaId) =>
        _context.Pujas.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.SubastaId == subastaId && p.EmpresaId == empresaId);

    public async Task<Puja> AddPujaAsync(Puja puja) { _context.Pujas.Add(puja); await _context.SaveChangesAsync(); return puja; }
    public Task<Puja?> GetPujaByIdAsync(Guid pujaId) => _context.Pujas.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == pujaId);

    public Task<List<Puja>> ListarPujasSubastaAsync(Guid subastaId) =>
        _context.Pujas.IgnoreQueryFilters().Include(p => p.Empresa)
            .Where(p => p.SubastaId == subastaId).OrderBy(p => p.Monto).ToListAsync();

    public Task SaveChangesAsync() => _context.SaveChangesAsync();

    public Task<Subasta?> GetByIdConDetalleAsync(Guid empresaId, Guid id) =>
        _context.Subastas.Include(s => s.TipoVehiculo).Include(s => s.OrigenDireccion).ThenInclude(d => d!.Ciudad)
            .Include(s => s.DestinoDireccion).ThenInclude(d => d!.Ciudad)
            .FirstOrDefaultAsync(s => s.Id == id && s.EmpresaId == empresaId);

    public Task<Empresa?> GetEmpresaAsync(Guid empresaId) => _context.Empresas.FirstOrDefaultAsync(e => e.Id == empresaId);

    public async Task<Guid> AdjudicarAsync(Subasta subasta, Puja ganadora, List<Puja> pujas, Guid empresaGanadora, string clienteNombre)
    {
        await using var tx = await _context.Database.BeginTransactionAsync();
        // cliente en empresa ganadora (reusar por nombre+empresa si existe)
        var cliente = await _context.Clientes.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.EmpresaId == empresaGanadora && c.PrimerNombre == clienteNombre && c.TipoCliente == "Empresa");
        if (cliente is null)
        {
            cliente = new Cliente { Id = Guid.NewGuid(), EmpresaId = empresaGanadora, PrimerNombre = clienteNombre, PrimerApellido = "-", Telefono = "-", TipoCliente = "Empresa" };
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
        }
        var origen = await _context.Direcciones.IgnoreQueryFilters().FirstAsync(d => d.Id == subasta.OrigenDireccionId);
        var destino = await _context.Direcciones.IgnoreQueryFilters().FirstAsync(d => d.Id == subasta.DestinoDireccionId);
        var origenCopia = new Direccion { Id = Guid.NewGuid(), EmpresaId = empresaGanadora, ClienteId = cliente.Id, CiudadId = origen.CiudadId, CallePrincipal = origen.CallePrincipal, NumeroExterior = origen.NumeroExterior, ColoniaSector = origen.ColoniaSector, Referencia = origen.Referencia, Lat = origen.Lat, Lng = origen.Lng };
        var destinoCopia = new Direccion { Id = Guid.NewGuid(), EmpresaId = empresaGanadora, ClienteId = cliente.Id, CiudadId = destino.CiudadId, CallePrincipal = destino.CallePrincipal, NumeroExterior = destino.NumeroExterior, ColoniaSector = destino.ColoniaSector, Referencia = destino.Referencia, Lat = destino.Lat, Lng = destino.Lng };
        _context.Direcciones.AddRange(origenCopia, destinoCopia);
        await _context.SaveChangesAsync();

        var envio = new Envio
        {
            Id = Guid.NewGuid(), EmpresaId = empresaGanadora, ClienteId = cliente.Id, SubastaId = subasta.Id,
            OrigenDireccionId = origenCopia.Id, DestinoDireccionId = destinoCopia.Id, VehiculoId = null, ConductorId = ganadora.ConductorId,
            TipoServicio = TipoServicio.Carga, Estado = EstadoEnvio.Pendiente, PesoKg = subasta.PesoKg, VolumenM3 = subasta.VolumenM3,
            FechaProgramada = DateTimeOffset.UtcNow, Tarifa = ganadora.Monto, CostoExterno = 0
        };
        _context.Envios.Add(envio);

        foreach (var p in pujas)
        {
            if (p.Estado == EstadoPuja.Activa) p.Estado = p.Id == ganadora.Id ? EstadoPuja.Ganadora : EstadoPuja.Perdedora;
        }
        subasta.Estado = EstadoSubasta.Adjudicada;
        await _context.SaveChangesAsync();
        await tx.CommitAsync();
        return envio.Id;
    }
}