using quickgo.Application.Common;
using quickgo.Application.Features.Subastas;
using quickgo.Application.Features.Subastas.DTOs;
using quickgo.Application.Interfaces;
using quickgo.Domain.Constants;
using quickgo.Domain.entities;
using quickgo.Domain.Enums;
using quickgo.Domain.enums;

namespace quickgo.Application.Services;

public class SubastaPujaService
{
    private readonly ISubastaRepository _repo;
    private readonly SubastaCierreService _cierre;
    private readonly ITenantContext _tenant;
    private readonly IConductorRepository _conductores;

    public SubastaPujaService(ISubastaRepository repo, SubastaCierreService cierre, ITenantContext tenant, IConductorRepository conductores)
    { _repo = repo; _cierre = cierre; _tenant = tenant; _conductores = conductores; }

    private bool TryEmpresa(out Guid empresaId) { empresaId = _tenant.EmpresaId ?? Guid.Empty; return _tenant.EmpresaId.HasValue; }

    public async Task<Resultado<List<SubastaAbiertaResponse>>> ListarAbiertasAsync(Guid? tipoVehiculoId, CancellationToken ct)
    {
        if (!TryEmpresa(out var empresaId)) return Resultado<List<SubastaAbiertaResponse>>.NoAutenticado("Token inválido", "El token no tiene una empresa.");
        await _cierre.AplicarTodasVencidasAsync();
        var subastas = await _repo.ListarAbiertasParaEmpresaAsync(empresaId, tipoVehiculoId);
        var ids = subastas.Select(s => s.Id).ToList();
        var mejores = await _repo.ObtenerMejoresOfertasAsync(ids);
        var yaPuje = await _repo.ObtenerSubastasPujadasPorEmpresaAsync(empresaId, ids);
        var resp = subastas.Select(s => new SubastaAbiertaResponse
        {
            Id = s.Id, TipoVehiculoId = s.TipoVehiculoId, TipoVehiculo = s.TipoVehiculo?.Nombre ?? "",
            OrigenCiudad = s.OrigenDireccion?.Ciudad?.Nombre ?? "", DestinoCiudad = s.DestinoDireccion?.Ciudad?.Nombre ?? "",
            Descripcion = s.Descripcion, PesoKg = s.PesoKg, VolumenM3 = s.VolumenM3, PrecioMaximo = s.PrecioMaximo,
            MonedaCodigo = s.MonedaCodigo, FechaCierre = s.FechaCierre, MejorOferta = mejores.TryGetValue(s.Id, out var m) ? m : null, YaPuje = yaPuje.Contains(s.Id)
        }).ToList();
        return Resultado<List<SubastaAbiertaResponse>>.Ok(resp);
    }

    public async Task<Resultado<PujaResponse>> PujarAsync(Guid subastaId, CrearPujaRequest r, CancellationToken ct)
    {
        if (!TryEmpresa(out var empresaId)) return Resultado<PujaResponse>.NoAutenticado("Token inválido", "El token no tiene una empresa.");
        var subasta = await _repo.GetAbiertaByIdAsync(subastaId);
        if (subasta is null) return Resultado<PujaResponse>.NoEncontrado("No encontrada", "La subasta no existe.");
        await _cierre.AplicarAsync(subasta);
        if (subasta.Estado != EstadoSubasta.Abierta) return Resultado<PujaResponse>.Conflicto("Transición inválida", "La subasta no está abierta.");
        if (subasta.EmpresaId == empresaId) return Resultado<PujaResponse>.Conflicto("No permitido", "No puedes pujar tu propia subasta.");
        if (r.Monto > subasta.PrecioMaximo) return Resultado<PujaResponse>.Invalido("Monto", "El monto no puede ser mayor al precio máximo.");
        if (await _repo.GetPujaEmpresaAsync(subastaId, empresaId) is not null)
            return Resultado<PujaResponse>.Conflicto("Puja duplicada", "Tu empresa ya tiene una puja en esta subasta.");
        var conductor = await _repo.GetConductorActivoDeEmpresaAsync(empresaId, r.ConductorId);
        if (conductor is null) return Resultado<PujaResponse>.Invalido("ConductorId", "El conductor no es de tu empresa o no está activo.");
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        if (!await _repo.EmpresaTieneVehiculoElegibleAsync(empresaId, subasta.TipoVehiculoId, hoy))
            return Resultado<PujaResponse>.Invalido("TipoVehiculoId", "Tu empresa no tiene un vehículo elegible con seguro vigente para este tipo.");
        var puja = new Puja { Id = Guid.NewGuid(), SubastaId = subastaId, EmpresaId = empresaId, ConductorId = r.ConductorId, Monto = r.Monto, Mensaje = string.IsNullOrWhiteSpace(r.Mensaje) ? null : r.Mensaje.Trim(), Estado = EstadoPuja.Activa };
        await _repo.AddPujaAsync(puja);
        return Resultado<PujaResponse>.Ok(ToPuja(puja));
    }

    public async Task<Resultado<MejorOfertaResponse>> MejorOfertaAsync(Guid subastaId, CancellationToken ct)
    {
        if (!TryEmpresa(out var empresaId)) return Resultado<MejorOfertaResponse>.NoAutenticado("Token inválido", "El token no tiene una empresa.");
        var subasta = await _repo.GetAbiertaByIdAsync(subastaId);
        if (subasta is null) return Resultado<MejorOfertaResponse>.NoEncontrado("No encontrada", "La subasta no existe.");
        await _cierre.AplicarAsync(subasta);
        var mejores = await _repo.ObtenerMejoresOfertasAsync(new[] { subastaId });
        var mia = await _repo.GetPujaEmpresaAsync(subastaId, empresaId);
        return Resultado<MejorOfertaResponse>.Ok(new MejorOfertaResponse { SubastaId = subastaId, MonedaCodigo = subasta.MonedaCodigo, Monto = mejores.TryGetValue(subastaId, out var m) ? m : null, EsMia = mia is not null && mia.Estado == EstadoPuja.Activa && mejores.TryGetValue(subastaId, out var mm) && mia.Monto == mm });
    }

    public async Task<Resultado<bool>> RetirarAsync(Guid subastaId, CancellationToken ct)
    {
        if (!TryEmpresa(out var empresaId)) return Resultado<bool>.NoAutenticado("Token inválido", "El token no tiene una empresa.");
        var subasta = await _repo.GetAbiertaByIdAsync(subastaId);
        if (subasta is null) return Resultado<bool>.NoEncontrado("No encontrada", "La subasta no existe.");
        await _cierre.AplicarAsync(subasta);
        if (subasta.Estado != EstadoSubasta.Abierta) return Resultado<bool>.Conflicto("Transición inválida", "Solo se puede retirar una puja mientras la subasta está Abierta.");
        var puja = await _repo.GetPujaEmpresaAsync(subastaId, empresaId);
        if (puja is null || puja.Estado != EstadoPuja.Activa) return Resultado<bool>.NoEncontrado("No encontrada", "No tienes una puja activa en esta subasta.");
        puja.Estado = EstadoPuja.Retirada;
        await _repo.SaveChangesAsync();
        return Resultado<bool>.Ok(true);
    }

    public async Task<Resultado<List<PujaDetalleResponse>>> VerPujasAsync(Guid subastaId, CancellationToken ct)
    {
        if (!TryEmpresa(out var empresaId)) return Resultado<List<PujaDetalleResponse>>.NoAutenticado("Token inválido", "El token no tiene una empresa.");
        var subasta = await _repo.GetByIdConDetalleAsync(empresaId, subastaId);
        if (subasta is null) return Resultado<List<PujaDetalleResponse>>.NoEncontrado("No encontrada", "La subasta no existe.");
        await _cierre.AplicarAsync(subasta);
        var pujas = await _repo.ListarPujasSubastaAsync(subastaId);
        var resp = new List<PujaDetalleResponse>();
        foreach (var p in pujas)
        {
            var conductor = await _conductores.GetByIdAsync(p.ConductorId);
            resp.Add(new PujaDetalleResponse { Id = p.Id, SubastaId = p.SubastaId, EmpresaId = p.EmpresaId, EmpresaNombre = p.Empresa?.NombreComercial ?? "", ConductorId = p.ConductorId, ConductorNombre = conductor?.Usuario?.NombreCompleto ?? "", Monto = p.Monto, Mensaje = p.Mensaje, Estado = p.Estado });
        }
        return Resultado<List<PujaDetalleResponse>>.Ok(resp);
    }

    public async Task<Resultado<SubastaResponseDto>> AdjudicarAsync(Guid subastaId, AdjudicarRequest r, CancellationToken ct)
    {
        if (!TryEmpresa(out var empresaId)) return Resultado<SubastaResponseDto>.NoAutenticado("Token inválido", "El token no tiene una empresa.");
        var subasta = await _repo.GetByIdConDetalleAsync(empresaId, subastaId);
        if (subasta is null) return Resultado<SubastaResponseDto>.NoEncontrado("No encontrada", "La subasta no existe.");
        await _cierre.AplicarAsync(subasta);
        if (subasta.Estado != EstadoSubasta.Abierta) return Resultado<SubastaResponseDto>.Conflicto("Transición inválida", "Solo se puede adjudicar una subasta Abierta.");
        var pujas = await _repo.ListarPujasSubastaAsync(subastaId);
        var ganadora = pujas.FirstOrDefault(p => p.Id == r.PujaId);
        if (ganadora is null) return Resultado<SubastaResponseDto>.NoEncontrado("No encontrada", "La puja no existe en esta subasta.");
        if (ganadora.Estado != EstadoPuja.Activa) return Resultado<SubastaResponseDto>.Conflicto("Transición inválida", "La puja seleccionada no está activa.");

        // adjudicación: crear/obtener cliente y direcciones en empresa ganadora y crear envio; actualizar estados.
        var empresaGanadora = ganadora.EmpresaId;
        var clienteNombre = subasta.Empresa?.NombreComercial ?? "Cliente subasta";
        var envioId = await _repo.AdjudicarAsync(subasta, ganadora, pujas, empresaGanadora, clienteNombre);

        var resp = new SubastaResponseDto { Id = subasta.Id, TipoVehiculoId = subasta.TipoVehiculoId, TipoVehiculo = subasta.TipoVehiculo?.Nombre ?? "", OrigenDireccionId = subasta.OrigenDireccionId, DestinoDireccionId = subasta.DestinoDireccionId, Descripcion = subasta.Descripcion, PesoKg = subasta.PesoKg, VolumenM3 = subasta.VolumenM3, PrecioMaximo = subasta.PrecioMaximo, MonedaCodigo = subasta.MonedaCodigo, FechaCierre = subasta.FechaCierre, Estado = EstadoSubasta.Adjudicada, TotalPujas = pujas.Count, EnvioId = envioId };
        return Resultado<SubastaResponseDto>.Ok(resp);
    }

    private static PujaResponse ToPuja(Puja p) => new() { Id = p.Id, SubastaId = p.SubastaId, ConductorId = p.ConductorId, Monto = p.Monto, Mensaje = p.Mensaje, Estado = p.Estado };
}
