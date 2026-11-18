using System.Security.Cryptography;
using quickgo.Application.Common;
using quickgo.Application.Features.Identidad;
using quickgo.Application.Features.Identidad.DTOs;
using quickgo.Application.Interfaces;
using quickgo.Domain.Constants;
using quickgo.Domain.entities;
using quickgo.Domain.enums;

namespace quickgo.Application.Services;

/// <summary>B1-04 y B1-05: registro, consulta y licencias por vencer de conductores.</summary>
public class ConductorService {
    private readonly IConductorRepository _conductores;
    private readonly IUsuarioRepository _usuarios;
    private readonly IRolRepository _roles;
    private readonly IEmpresaRepository _empresas;
    private readonly IPasswordHasher _hasher;
    private readonly IUnitOfWork _uow;
    private readonly ITenantContext _tenant;

    public ConductorService(IConductorRepository conductores, IUsuarioRepository usuarios, IRolRepository roles,
        IEmpresaRepository empresas, IPasswordHasher hasher, IUnitOfWork uow, ITenantContext tenant) {
        _conductores = conductores;
        _usuarios = usuarios;
        _roles = roles;
        _empresas = empresas;
        _hasher = hasher;
        _uow = uow;
        _tenant = tenant;
    }

    public async Task<Resultado<ConductorResponse>> CrearAsync(CrearConductorRequest request) {
        if (_tenant.EmpresaId is not Guid empresaId) {
            return Resultado<ConductorResponse>.NoAutenticado("Token inválido", "El token no tiene una empresa.");
        }

        var empresa = await _empresas.GetByIdAsync(empresaId);
        if (empresa is null || empresa.Tipo != TipoEmpresa.Transportista) {
            return Resultado<ConductorResponse>.Invalido("TipoConductor",
                "Solo las empresas Transportista pueden registrar conductores.");
        }

        var licencia = request.Licencia.Trim();
        if (await _conductores.ExisteLicenciaAsync(licencia, null)) {
            return Resultado<ConductorResponse>.Conflicto("Registro duplicado",
                "La licencia ya está registrada en tu empresa.");
        }

        var rolConductor = await _roles.GetByNombreAsync(Roles.Conductor)
            ?? throw new InvalidOperationException("El rol Conductor no existe. Aplica las migraciones.");

        var id = Guid.NewGuid();
        var externo = request.TipoConductor == TipoConductor.Externo;

        string email;
        string password;
        if (externo) {
            // Sin acceso al sistema: correo interno unico y clave aleatoria que nadie conoce.
            email = $"externo-{id:N}@conductores.quickgo.local";
            password = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        } else {
            email = ReglasIdentidad.NormalizarEmail(request.Email!);
            password = request.Password!;
            if (await _usuarios.ExisteEmailAsync(email)) {
                return Resultado<ConductorResponse>.Conflicto("Registro duplicado", "El email ya está registrado.");
            }
        }

        var usuario = new Usuario {
            Id = id,
            EmpresaId = empresaId,
            RolId = rolConductor.Id,
            Rol = rolConductor,
            PrimerNombre = request.PrimerNombre.Trim(),
            SegundoNombre = Mapeos.LimpiarOpcional(request.SegundoNombre),
            PrimerApellido = request.PrimerApellido.Trim(),
            SegundoApellido = Mapeos.LimpiarOpcional(request.SegundoApellido),
            Telefono = Mapeos.LimpiarOpcional(request.Telefono),
            Email = email,
            PasswordHash = _hasher.Hash(password),
            Activo = !externo
        };

        var conductor = new Conductor {
            Id = id,
            Usuario = usuario,
            EmpresaId = empresaId,
            TipoConductor = request.TipoConductor,
            Licencia = licencia,
            VenceLicencia = request.VenceLicencia,
            Estado = EstadoConductor.Activo
        };

        _usuarios.Add(usuario);
        _conductores.Add(conductor);

        try {
            await _uow.GuardarAsync(); // usuario + conductor en una sola transaccion
        } catch (ConflictoException ex) {
            return ReglasIdentidad.MensajeConflicto<ConductorResponse>(ex);
        }

        return Resultado<ConductorResponse>.Ok(conductor.ToResponse());
    }

    public async Task<IReadOnlyList<ConductorResponse>> ListarAsync(ConductorFiltroQuery filtro) {
        // El rol Conductor solo ve su propio perfil.
        Guid? soloId = _tenant.Rol == Roles.Conductor ? (_tenant.UserId ?? Guid.Empty) : null;
        var conductores = await _conductores.ListarAsync(filtro.Estado, filtro.TipoConductor, soloId);
        return conductores.Select(c => c.ToResponse()).ToList();
    }

    public async Task<Resultado<ConductorResponse>> ObtenerAsync(Guid id) {
        if (_tenant.Rol == Roles.Conductor && _tenant.UserId != id) {
            return NoExiste();
        }

        var conductor = await _conductores.GetByIdAsync(id);
        return conductor is null ? NoExiste() : Resultado<ConductorResponse>.Ok(conductor.ToResponse());
    }

    public async Task<Resultado<ConductorResponse>> ActualizarAsync(Guid id, ActualizarConductorRequest request) {
        var conductor = await _conductores.GetByIdAsync(id);
        if (conductor is null || conductor.Usuario is null) {
            return NoExiste();
        }

        var licencia = request.Licencia.Trim();
        if (await _conductores.ExisteLicenciaAsync(licencia, id)) {
            return Resultado<ConductorResponse>.Conflicto("Registro duplicado",
                "La licencia ya está registrada en tu empresa.");
        }

        var usuario = conductor.Usuario;
        usuario.PrimerNombre = request.PrimerNombre.Trim();
        usuario.SegundoNombre = Mapeos.LimpiarOpcional(request.SegundoNombre);
        usuario.PrimerApellido = request.PrimerApellido.Trim();
        usuario.SegundoApellido = Mapeos.LimpiarOpcional(request.SegundoApellido);
        usuario.Telefono = Mapeos.LimpiarOpcional(request.Telefono);

        conductor.Licencia = licencia;
        conductor.VenceLicencia = request.VenceLicencia;
        conductor.Estado = request.Estado;

        try {
            await _uow.GuardarAsync();
        } catch (ConflictoException ex) {
            return ReglasIdentidad.MensajeConflicto<ConductorResponse>(ex);
        }

        return Resultado<ConductorResponse>.Ok(conductor.ToResponse());
    }

    public async Task<IReadOnlyList<LicenciaPorVencerResponse>> LicenciasPorVencerAsync(int dias) {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var conductores = await _conductores.ListarConLicenciaHastaAsync(hoy.AddDays(dias));

        // Incluye las ya vencidas (DiasRestantes negativo), ordenadas por vencimiento ascendente.
        return conductores
            .OrderBy(c => c.VenceLicencia)
            .Select(c => new LicenciaPorVencerResponse(
                c.Id,
                c.Usuario?.NombreCompleto ?? string.Empty,
                c.Licencia,
                c.VenceLicencia,
                c.VenceLicencia.DayNumber - hoy.DayNumber,
                c.VenceLicencia < hoy))
            .ToList();
    }

    private static Resultado<ConductorResponse> NoExiste() =>
        Resultado<ConductorResponse>.NoEncontrado("Conductor no encontrado", "El conductor no existe.");
}
