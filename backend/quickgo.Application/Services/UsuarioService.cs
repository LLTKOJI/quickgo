using quickgo.Application.Common;
using quickgo.Application.Features.Identidad;
using quickgo.Application.Features.Identidad.DTOs;
using quickgo.Application.Interfaces;
using quickgo.Domain.Constants;
using quickgo.Domain.entities;

namespace quickgo.Application.Services;

/// <summary>B1-03: gestion de usuarios de la empresa del administrador.</summary>
public class UsuarioService {
    private readonly IUsuarioRepository _usuarios;
    private readonly IRolRepository _roles;
    private readonly IEmpresaRepository _empresas;
    private readonly IPasswordHasher _hasher;
    private readonly IUnitOfWork _uow;
    private readonly ITenantContext _tenant;

    public UsuarioService(IUsuarioRepository usuarios, IRolRepository roles, IEmpresaRepository empresas,
        IPasswordHasher hasher, IUnitOfWork uow, ITenantContext tenant) {
        _usuarios = usuarios;
        _roles = roles;
        _empresas = empresas;
        _hasher = hasher;
        _uow = uow;
        _tenant = tenant;
    }

    public async Task<Resultado<UsuarioResponse>> CrearAsync(CrearUsuarioRequest request) {
        if (_tenant.EmpresaId is not Guid empresaId) {
            return SinEmpresa();
        }

        var email = ReglasIdentidad.NormalizarEmail(request.Email);
        if (await _usuarios.ExisteEmailAsync(email)) {
            return Resultado<UsuarioResponse>.Conflicto("Registro duplicado", "El email ya está registrado.");
        }

        var rol = await ValidarRolAsync(empresaId, request.RolId);
        if (rol.Error is not null) {
            return Resultado<UsuarioResponse>.Invalido("RolId", rol.Mensaje!);
        }

        var usuario = new Usuario {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            RolId = rol.Rol!.Id,
            Rol = rol.Rol,
            PrimerNombre = request.PrimerNombre.Trim(),
            SegundoNombre = Mapeos.LimpiarOpcional(request.SegundoNombre),
            PrimerApellido = request.PrimerApellido.Trim(),
            SegundoApellido = Mapeos.LimpiarOpcional(request.SegundoApellido),
            Telefono = Mapeos.LimpiarOpcional(request.Telefono),
            Email = email,
            PasswordHash = _hasher.Hash(request.Password),
            Activo = true
        };

        _usuarios.Add(usuario);

        try {
            await _uow.GuardarAsync();
        } catch (ConflictoException ex) {
            return ReglasIdentidad.MensajeConflicto<UsuarioResponse>(ex);
        }

        return Resultado<UsuarioResponse>.Ok(usuario.ToResponse());
    }

    public async Task<IReadOnlyList<UsuarioResponse>> ListarAsync() {
        var usuarios = await _usuarios.ListarAsync();
        return usuarios.Select(u => u.ToResponse()).ToList();
    }

    public async Task<Resultado<UsuarioResponse>> ActualizarAsync(Guid id, ActualizarUsuarioRequest request) {
        if (_tenant.EmpresaId is not Guid empresaId) {
            return SinEmpresa();
        }

        var usuario = await _usuarios.GetByIdAsync(id);
        if (usuario is null) {
            return NoExiste();
        }

        var rol = await ValidarRolAsync(empresaId, request.RolId);
        if (rol.Error is not null) {
            return Resultado<UsuarioResponse>.Invalido("RolId", rol.Mensaje!);
        }

        // No se le puede quitar el rol Admin al ultimo Admin activo.
        var eraAdmin = usuario.Rol?.Nombre == Roles.Admin;
        if (eraAdmin && usuario.Activo && rol.Rol!.Nombre != Roles.Admin
            && await _usuarios.ContarAdminsActivosAsync() <= 1) {
            return Resultado<UsuarioResponse>.Conflicto("Operación no permitida",
                "No se puede quitar el rol Admin al último administrador de la empresa.");
        }

        usuario.PrimerNombre = request.PrimerNombre.Trim();
        usuario.SegundoNombre = Mapeos.LimpiarOpcional(request.SegundoNombre);
        usuario.PrimerApellido = request.PrimerApellido.Trim();
        usuario.SegundoApellido = Mapeos.LimpiarOpcional(request.SegundoApellido);
        usuario.Telefono = Mapeos.LimpiarOpcional(request.Telefono);
        usuario.RolId = rol.Rol!.Id;
        usuario.Rol = rol.Rol;

        await _uow.GuardarAsync();
        return Resultado<UsuarioResponse>.Ok(usuario.ToResponse());
    }

    public async Task<Resultado<UsuarioResponse>> DesactivarAsync(Guid id) {
        var usuario = await _usuarios.GetByIdAsync(id);
        if (usuario is null) {
            return NoExiste();
        }

        if (_tenant.UserId == id) {
            return Resultado<UsuarioResponse>.Conflicto("Operación no permitida",
                "No puedes desactivar tu propio usuario.");
        }

        if (usuario.Activo && usuario.Rol?.Nombre == Roles.Admin
            && await _usuarios.ContarAdminsActivosAsync() <= 1) {
            return Resultado<UsuarioResponse>.Conflicto("Operación no permitida",
                "No se puede desactivar al último administrador de la empresa.");
        }

        usuario.Activo = false; // baja logica, nunca se borra
        await _uow.GuardarAsync();
        return Resultado<UsuarioResponse>.Ok(usuario.ToResponse());
    }

    private async Task<(Rol? Rol, string? Error, string? Mensaje)> ValidarRolAsync(Guid empresaId, Guid rolId) {
        var rol = await _roles.GetByIdAsync(rolId);
        if (rol is null) {
            return (null, "rol", "El rol no existe.");
        }

        var empresa = await _empresas.GetByIdAsync(empresaId);
        if (empresa is null) {
            return (null, "empresa", "La empresa no existe.");
        }

        if (!ReglasIdentidad.RolCompatible(rol.Nombre, empresa.Tipo)) {
            return (null, "rol", $"El rol {rol.Nombre} no está permitido en una empresa {empresa.Tipo}.");
        }

        return (rol, null, null);
    }

    private static Resultado<UsuarioResponse> NoExiste() =>
        Resultado<UsuarioResponse>.NoEncontrado("Usuario no encontrado", "El usuario no existe.");

    private static Resultado<UsuarioResponse> SinEmpresa() =>
        Resultado<UsuarioResponse>.NoAutenticado("Token inválido", "El token no tiene una empresa.");
}
