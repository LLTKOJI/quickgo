using quickgo.Application.Common;
using quickgo.Application.Features.Envios.DTOs;
using quickgo.Application.Interfaces;
using quickgo.Domain.Constants;
using quickgo.Domain.entities;
using quickgo.Domain.Enums;

namespace quickgo.Application.Services;

public class EnvioService
{
    private readonly IEnvioRepository _repo;
    private readonly ITenantContext _tenant;
    private readonly IUsuarioRepository _usuarios;
    private readonly IConductorRepository _conductores;

    public EnvioService(IEnvioRepository repo, ITenantContext tenant, IUsuarioRepository usuarios, IConductorRepository conductores)
    {
        _repo = repo;
        _tenant = tenant;
        _usuarios = usuarios;
        _conductores = conductores;
    }

    private bool TryEmpresa(out Guid empresaId)
    {
        empresaId = _tenant.EmpresaId ?? Guid.Empty;
        return _tenant.EmpresaId.HasValue;
    }

    private async Task<Guid?> ResolverConductorIdAsync(Guid empresaId, CancellationToken ct)
    {
        if (_tenant.Rol != Roles.Conductor) return null;
        if (_tenant.UserId is not Guid uid) return Guid.Empty;
        var c = await _conductores.GetByIdAsync(uid);
        return c?.Id ?? Guid.Empty;
    }

    public async Task<Resultado<EnvioResponse>> CrearAsync(CrearEnvioRequest r, CancellationToken ct)
    {
        if (!TryEmpresa(out var empresaId)) return Resultado<EnvioResponse>.NoAutenticado("Token inválido", "El token no tiene una empresa.");
        var err = await ValidarRefsAsync(empresaId, r.ClienteId, r.OrigenDireccionId, r.DestinoDireccionId, ct);
        if (err is not null) return err;
        var envio = new Envio
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            ClienteId = r.ClienteId,
            OrigenDireccionId = r.OrigenDireccionId,
            DestinoDireccionId = r.DestinoDireccionId,
            TipoServicio = r.TipoServicio,
            FechaProgramada = r.FechaProgramada.ToUniversalTime(),
            PesoKg = r.PesoKg,
            VolumenM3 = r.VolumenM3,
            Tarifa = r.Tarifa,
            CostoExterno = 0,
            Estado = EstadoEnvio.Pendiente
        };
        await _repo.AddAsync(envio, ct);
        return Resultado<EnvioResponse>.Ok(await MapAsync(envio, ct));
    }

    public async Task<Resultado<List<EnvioResponse>>> ListarAsync(EnvioFiltroQuery f, CancellationToken ct)
    {
        if (!TryEmpresa(out var empresaId)) return Resultado<List<EnvioResponse>>.NoAutenticado("Token inválido", "El token no tiene una empresa.");
        if (f.Desde.HasValue && f.Hasta.HasValue && f.Desde > f.Hasta)
            return Resultado<List<EnvioResponse>>.Invalido("Desde", "Desde no puede ser mayor que Hasta.");
        Guid? conductorId = await ResolverConductorIdAsync(empresaId, ct);
        if (conductorId == Guid.Empty) return Resultado<List<EnvioResponse>>.Ok(new());
        var list = await _repo.ListarAsync(empresaId, f.Estado, f.Desde, f.Hasta, conductorId, ct);
        var resp = new List<EnvioResponse>(list.Count);
        foreach (var e in list) resp.Add(await MapAsync(e, ct));
        return Resultado<List<EnvioResponse>>.Ok(resp);
    }

    public async Task<Resultado<EnvioDetalleResponse>> ObtenerAsync(Guid id, CancellationToken ct)
    {
        if (!TryEmpresa(out var empresaId)) return Resultado<EnvioDetalleResponse>.NoAutenticado("Token inválido", "El token no tiene una empresa.");
        var envio = await _repo.GetByIdAsync(empresaId, id, ct);
        if (envio is null) return NoEncontrado<EnvioDetalleResponse>();
        var conductorId = await ResolverConductorIdAsync(empresaId, ct);
        if (conductorId.HasValue && conductorId != Guid.Empty && envio.ConductorId != conductorId) return NoEncontrado<EnvioDetalleResponse>();
        if (conductorId == Guid.Empty) return NoEncontrado<EnvioDetalleResponse>();
        return Resultado<EnvioDetalleResponse>.Ok(await MapDetalleAsync(envio, ct));
    }

    public async Task<Resultado<EnvioResponse>> ActualizarAsync(Guid id, ActualizarEnvioRequest r, CancellationToken ct)
    {
        if (!TryEmpresa(out var empresaId)) return Resultado<EnvioResponse>.NoAutenticado("Token inválido", "El token no tiene una empresa.");
        var envio = await _repo.GetByIdAsync(empresaId, id, ct);
        if (envio is null) return NoEncontrado<EnvioResponse>();
        if (envio.Estado != EstadoEnvio.Pendiente) return Resultado<EnvioResponse>.Conflicto("Transición inválida", "Solo se puede editar un envío Pendiente.");
        var err = await ValidarRefsAsync(empresaId, r.ClienteId, r.OrigenDireccionId, r.DestinoDireccionId, ct);
        if (err is not null) return err;
        envio.ClienteId = r.ClienteId;
        envio.OrigenDireccionId = r.OrigenDireccionId;
        envio.DestinoDireccionId = r.DestinoDireccionId;
        envio.TipoServicio = r.TipoServicio;
        envio.FechaProgramada = r.FechaProgramada.ToUniversalTime();
        envio.PesoKg = r.PesoKg;
        envio.VolumenM3 = r.VolumenM3;
        envio.Tarifa = r.Tarifa;
        await _repo.UpdateAsync(envio, ct);
        return Resultado<EnvioResponse>.Ok(await MapAsync(envio, ct));
    }

    public async Task<Resultado<EnvioResponse>> CancelarAsync(Guid id, CancellationToken ct)
    {
        if (!TryEmpresa(out var empresaId)) return Resultado<EnvioResponse>.NoAutenticado("Token inválido", "El token no tiene una empresa.");
        var envio = await _repo.GetByIdAsync(empresaId, id, ct);
        if (envio is null) return NoEncontrado<EnvioResponse>();
        if (envio.Estado is not (EstadoEnvio.Pendiente or EstadoEnvio.Asignado))
            return Resultado<EnvioResponse>.Conflicto("Transición inválida", "Solo se puede cancelar un envío Pendiente o Asignado.");
        envio.Estado = EstadoEnvio.Cancelado;
        await _repo.UpdateAsync(envio, ct);
        return Resultado<EnvioResponse>.Ok(await MapAsync(envio, ct));
    }

    public async Task<Resultado<EnvioResponse>> AsignarAsync(Guid id, AsignarEnvioRequest r, CancellationToken ct)
    {
        if (!TryEmpresa(out var empresaId)) return Resultado<EnvioResponse>.NoAutenticado("Token inválido", "El token no tiene una empresa.");
        var envio = await _repo.GetByIdAsync(empresaId, id, ct);
        if (envio is null) return NoEncontrado<EnvioResponse>();
        if (envio.Estado != EstadoEnvio.Pendiente) return Resultado<EnvioResponse>.Conflicto("Transición inválida", "Solo se puede asignar un envío Pendiente.");
        var vehiculo = await _repo.GetVehiculoAsync(empresaId, r.VehiculoId, ct);
        if (vehiculo is null) return Resultado<EnvioResponse>.Invalido("VehiculoId", "El vehículo no es de tu empresa.");
        if (vehiculo.Estado == EstadoVehiculo.Baja) return Resultado<EnvioResponse>.Invalido("VehiculoId", "El vehículo está dado de baja.");
        var conductor = await _repo.GetConductorAsync(empresaId, r.ConductorId, ct);
        if (conductor is null) return Resultado<EnvioResponse>.Invalido("ConductorId", "El conductor no es de tu empresa.");
        if (conductor.Estado != quickgo.Domain.enums.EstadoConductor.Activo) return Resultado<EnvioResponse>.Invalido("ConductorId", "El conductor no está activo.");
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        if (!await _repo.VehiculoTieneSeguroVigenteAsync(empresaId, r.VehiculoId, hoy, ct))
            return Resultado<EnvioResponse>.Invalido("VehiculoId", "El vehículo no tiene seguro vigente.");
        envio.VehiculoId = r.VehiculoId;
        envio.ConductorId = r.ConductorId;
        envio.CostoExterno = r.CostoExterno ?? envio.CostoExterno;
        envio.Estado = EstadoEnvio.Asignado;
        await _repo.UpdateAsync(envio, ct);
        return Resultado<EnvioResponse>.Ok(await MapAsync(envio, ct));
    }

    public async Task<Resultado<EnvioResponse>> CambiarEstadoAsync(Guid id, CambiarEstadoRequest r, CancellationToken ct)
    {
        if (!TryEmpresa(out var empresaId)) return Resultado<EnvioResponse>.NoAutenticado("Token inválido", "El token no tiene una empresa.");
        var envio = await _repo.GetByIdAsync(empresaId, id, ct);
        if (envio is null) return NoEncontrado<EnvioResponse>();
        var esConductor = _tenant.Rol == Roles.Conductor;
        var conductorId = await ResolverConductorIdAsync(empresaId, ct);
        if (esConductor)
        {
            if (conductorId == Guid.Empty || envio.ConductorId != conductorId) return NoEncontrado<EnvioResponse>();
            if (r.Estado is not (EstadoEnvio.EnCurso or EstadoEnvio.Completado))
                return Resultado<EnvioResponse>.Prohibido("Acceso denegado", "El conductor solo puede marcar EnCurso o Completado.");
        }
        if (!TransicionValida(envio.Estado, r.Estado))
            return Resultado<EnvioResponse>.Conflicto("Transición inválida", $"No se puede pasar de {envio.Estado} a {r.Estado}.");
        envio.Estado = r.Estado;
        await _repo.UpdateAsync(envio, ct);
        await _repo.ActualizarEstadoVehiculoPorEnvioAsync(envio, ct);
        return Resultado<EnvioResponse>.Ok(await MapAsync(envio, ct));
    }

    public async Task<Resultado<CotizacionResponse>> CrearCotizacionAsync(Guid envioId, CrearCotizacionRequest r, CancellationToken ct)
    {
        if (!TryEmpresa(out var empresaId)) return Resultado<CotizacionResponse>.NoAutenticado("Token inválido", "El token no tiene una empresa.");
        var envio = await _repo.GetByIdAsync(empresaId, envioId, ct);
        if (envio is null) return NoEncontrado<CotizacionResponse>();
        if (envio.Estado is not (EstadoEnvio.Pendiente or EstadoEnvio.Asignado))
            return Resultado<CotizacionResponse>.Conflicto("Transición inválida", "Solo se puede cotizar un envío Pendiente o Asignado.");
        var moneda = r.MonedaCodigo.Trim().ToUpperInvariant();
        if (!await _repo.MonedaExisteAsync(moneda, ct)) return Resultado<CotizacionResponse>.Invalido("MonedaCodigo", "La moneda no existe.");
        var cot = new Cotizacion
        {
            Id = Guid.NewGuid(), EmpresaId = empresaId, EnvioId = envioId,
            Monto = r.Monto, MonedaCodigo = moneda, ValidaHasta = r.ValidaHasta.ToUniversalTime(), Aceptada = false
        };
        await _repo.AddCotizacionAsync(cot, ct);
        return Resultado<CotizacionResponse>.Ok(ToCotizacion(cot));
    }

    public async Task<Resultado<List<CotizacionResponse>>> ListarCotizacionesAsync(Guid envioId, CancellationToken ct)
    {
        if (!TryEmpresa(out var empresaId)) return Resultado<List<CotizacionResponse>>.NoAutenticado("Token inválido", "El token no tiene una empresa.");
        var envio = await _repo.GetByIdAsync(empresaId, envioId, ct);
        if (envio is null) return NoEncontrado<List<CotizacionResponse>>();
        var list = await _repo.ListarCotizacionesAsync(empresaId, envioId, ct);
        return Resultado<List<CotizacionResponse>>.Ok(list.Select(ToCotizacion).ToList());
    }

    public async Task<Resultado<CotizacionResponse>> AceptarCotizacionAsync(Guid envioId, Guid cotizacionId, CancellationToken ct)
    {
        if (!TryEmpresa(out var empresaId)) return Resultado<CotizacionResponse>.NoAutenticado("Token inválido", "El token no tiene una empresa.");
        var envio = await _repo.GetByIdAsync(empresaId, envioId, ct);
        if (envio is null) return NoEncontrado<CotizacionResponse>();
        var cot = await _repo.GetCotizacionAsync(empresaId, envioId, cotizacionId, ct);
        if (cot is null) return NoEncontrado<CotizacionResponse>();
        if (cot.ValidaHasta < DateTimeOffset.UtcNow) return Resultado<CotizacionResponse>.Conflicto("Cotización vencida", "No se puede aceptar una cotización vencida.");
        var todas = await _repo.ListarCotizacionesAsync(empresaId, envioId, ct);
        // recargar tracked entities
        var tracked = new List<Cotizacion>();
        foreach (var t in todas)
        {
            var e = await _repo.GetCotizacionAsync(empresaId, envioId, t.Id, ct);
            if (e != null) tracked.Add(e);
        }
        var aceptada = tracked.First(c => c.Id == cotizacionId);
        await _repo.AceptarCotizacionAsync(envio, aceptada, tracked, ct);
        return Resultado<CotizacionResponse>.Ok(ToCotizacion(aceptada));
    }

    public async Task<Resultado<SeguimientoResponse>> RegistrarSeguimientoAsync(Guid envioId, RegistrarSeguimientoRequest r, CancellationToken ct)
    {
        if (!TryEmpresa(out var empresaId)) return Resultado<SeguimientoResponse>.NoAutenticado("Token inválido", "El token no tiene una empresa.");
        var envio = await _repo.GetByIdAsync(empresaId, envioId, ct);
        if (envio is null) return NoEncontrado<SeguimientoResponse>();
        var conductorId = await ResolverConductorIdAsync(empresaId, ct);
        if (conductorId == Guid.Empty || envio.ConductorId != conductorId) return NoEncontrado<SeguimientoResponse>();
        if (envio.Estado != EstadoEnvio.EnCurso) return Resultado<SeguimientoResponse>.Conflicto("Transición inválida", "Solo se puede registrar seguimiento en envíos EnCurso.");
        var seg = new SeguimientoEnvio
        {
            Id = Guid.NewGuid(), EmpresaId = empresaId, EnvioId = envioId,
            Lat = r.Lat, Lng = r.Lng, Estado = r.Estado.Trim(), Momento = DateTimeOffset.UtcNow
        };
        await _repo.AddSeguimientoAsync(seg, ct);
        return Resultado<SeguimientoResponse>.Ok(ToSeg(seg));
    }

    public async Task<Resultado<List<SeguimientoResponse>>> ListarSeguimientoAsync(Guid envioId, CancellationToken ct)
    {
        if (!TryEmpresa(out var empresaId)) return Resultado<List<SeguimientoResponse>>.NoAutenticado("Token inválido", "El token no tiene una empresa.");
        var envio = await _repo.GetByIdAsync(empresaId, envioId, ct);
        if (envio is null) return NoEncontrado<List<SeguimientoResponse>>();
        var esConductor = _tenant.Rol == Roles.Conductor;
        if (esConductor)
        {
            var conductorId = await ResolverConductorIdAsync(empresaId, ct);
            if (conductorId == Guid.Empty || envio.ConductorId != conductorId) return NoEncontrado<List<SeguimientoResponse>>();
        }
        var list = await _repo.ListarSeguimientoAsync(empresaId, envioId, ct);
        return Resultado<List<SeguimientoResponse>>.Ok(list.Select(ToSeg).ToList());
    }

    public async Task<Resultado<SeguimientoResponse>> UltimoSeguimientoAsync(Guid envioId, CancellationToken ct)
    {
        if (!TryEmpresa(out var empresaId)) return Resultado<SeguimientoResponse>.NoAutenticado("Token inválido", "El token no tiene una empresa.");
        var envio = await _repo.GetByIdAsync(empresaId, envioId, ct);
        if (envio is null) return NoEncontrado<SeguimientoResponse>();
        var esConductor = _tenant.Rol == Roles.Conductor;
        if (esConductor)
        {
            var conductorId = await ResolverConductorIdAsync(empresaId, ct);
            if (conductorId == Guid.Empty || envio.ConductorId != conductorId) return NoEncontrado<SeguimientoResponse>();
        }
        var seg = await _repo.GetUltimoSeguimientoAsync(empresaId, envioId, ct);
        if (seg is null) return Resultado<SeguimientoResponse>.NoEncontrado("No encontrado", "El envío no tiene seguimiento.");
        return Resultado<SeguimientoResponse>.Ok(ToSeg(seg));
    }

    private async Task<Resultado<EnvioResponse>?> ValidarRefsAsync(Guid empresaId, Guid clienteId, Guid origenId, Guid destinoId, CancellationToken ct)
    {
        if (await _repo.GetClienteAsync(empresaId, clienteId, ct) is null)
            return Resultado<EnvioResponse>.Invalido("ClienteId", "El cliente no es de tu empresa.");
        if (await _repo.GetDireccionAsync(empresaId, origenId, ct) is null)
            return Resultado<EnvioResponse>.Invalido("OrigenDireccionId", "El origen no es una dirección de tu empresa.");
        if (await _repo.GetDireccionAsync(empresaId, destinoId, ct) is null)
            return Resultado<EnvioResponse>.Invalido("DestinoDireccionId", "El destino no es una dirección de tu empresa.");
        return null;
    }

    private static bool TransicionValida(EstadoEnvio actual, EstadoEnvio nuevo) => (actual, nuevo) switch
    {
        (EstadoEnvio.Pendiente, EstadoEnvio.Asignado) => true,
        (EstadoEnvio.Asignado, EstadoEnvio.EnCurso) => true,
        (EstadoEnvio.EnCurso, EstadoEnvio.Completado) => true,
        (EstadoEnvio.Pendiente, EstadoEnvio.Cancelado) => true,
        (EstadoEnvio.Asignado, EstadoEnvio.Cancelado) => true,
        _ => false
    };

    private async Task<EnvioResponse> MapAsync(Envio e, CancellationToken ct)
    {
        var cliente = await _repo.GetClienteAsync(_tenant.EmpresaId!.Value, e.ClienteId, ct);
        return new EnvioResponse
        {
            Id = e.Id, ClienteId = e.ClienteId, ClienteNombre = NombreCliente(cliente),
            SubastaId = e.SubastaId, OrigenDireccionId = e.OrigenDireccionId, DestinoDireccionId = e.DestinoDireccionId,
            VehiculoId = e.VehiculoId, ConductorId = e.ConductorId, TipoServicio = e.TipoServicio, Estado = e.Estado,
            PesoKg = e.PesoKg, VolumenM3 = e.VolumenM3, FechaProgramada = e.FechaProgramada, Tarifa = e.Tarifa, CostoExterno = e.CostoExterno
        };
    }

    private async Task<EnvioDetalleResponse> MapDetalleAsync(Envio e, CancellationToken ct)
    {
        var baseResp = await MapAsync(e, ct);
        var origen = await _repo.GetDireccionAsync(_tenant.EmpresaId!.Value, e.OrigenDireccionId, ct);
        var destino = await _repo.GetDireccionAsync(_tenant.EmpresaId!.Value, e.DestinoDireccionId, ct);
        var vehiculo = e.VehiculoId.HasValue ? await _repo.GetVehiculoAsync(_tenant.EmpresaId!.Value, e.VehiculoId.Value, ct) : null;
        var conductor = e.ConductorId.HasValue ? await _repo.GetConductorAsync(_tenant.EmpresaId!.Value, e.ConductorId.Value, ct) : null;
        return new EnvioDetalleResponse
        {
            Id = baseResp.Id, ClienteId = baseResp.ClienteId, ClienteNombre = baseResp.ClienteNombre, SubastaId = baseResp.SubastaId,
            OrigenDireccionId = baseResp.OrigenDireccionId, DestinoDireccionId = baseResp.DestinoDireccionId,
            VehiculoId = baseResp.VehiculoId, ConductorId = baseResp.ConductorId, TipoServicio = baseResp.TipoServicio, Estado = baseResp.Estado,
            PesoKg = baseResp.PesoKg, VolumenM3 = baseResp.VolumenM3, FechaProgramada = baseResp.FechaProgramada, Tarifa = baseResp.Tarifa, CostoExterno = baseResp.CostoExterno,
            Origen = ToDir(origen), Destino = ToDir(destino),
            VehiculoPlaca = vehiculo?.Placa, ConductorNombre = conductor?.Usuario?.NombreCompleto
        };
    }

    private static string NombreCliente(Cliente? c) => c is null ? "" : string.Join(" ", new[] { c.PrimerNombre, c.SegundoNombre, c.PrimerApellido, c.SegundoApellido }.Where(s => !string.IsNullOrWhiteSpace(s)));
    private static DireccionResumenResponse ToDir(Direccion? d) => d is null ? new() : new()
    {
        Id = d.Id, Lat = d.Lat, Lng = d.Lng,
        Texto = string.Join(", ", new[] { d.CallePrincipal, d.ColoniaSector, d.Ciudad?.Nombre }.Where(s => !string.IsNullOrWhiteSpace(s)))
    };
    private static CotizacionResponse ToCotizacion(Cotizacion c) => new()
    {
        Id = c.Id, EnvioId = c.EnvioId, Monto = c.Monto, MonedaCodigo = c.MonedaCodigo,
        ValidaHasta = c.ValidaHasta, Aceptada = c.Aceptada, Vencida = c.ValidaHasta < DateTimeOffset.UtcNow
    };
    private static SeguimientoResponse ToSeg(SeguimientoEnvio s) => new() { Id = s.Id, EnvioId = s.EnvioId, Lat = s.Lat, Lng = s.Lng, Estado = s.Estado, Momento = s.Momento };
    private static Resultado<T> NoEncontrado<T>() => Resultado<T>.NoEncontrado("No encontrado", "El envío no existe.");
}
