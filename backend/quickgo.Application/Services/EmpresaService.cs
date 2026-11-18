using quickgo.Application.Common;
using quickgo.Application.Features.Identidad;
using quickgo.Application.Features.Identidad.DTOs;
using quickgo.Application.Interfaces;
using quickgo.Domain.Constants;
using quickgo.Domain.entities;
using quickgo.Domain.enums;

namespace quickgo.Application.Services;

/// <summary>B1-01: registro de empresa con su usuario administrador (una sola transaccion).</summary>
public class EmpresaService {
    private readonly IEmpresaRepository _empresas;
    private readonly IUsuarioRepository _usuarios;
    private readonly IRolRepository _roles;
    private readonly IPasswordHasher _hasher;
    private readonly IUnitOfWork _uow;

    public EmpresaService(IEmpresaRepository empresas, IUsuarioRepository usuarios, IRolRepository roles,
        IPasswordHasher hasher, IUnitOfWork uow) {
        _empresas = empresas;
        _usuarios = usuarios;
        _roles = roles;
        _hasher = hasher;
        _uow = uow;
    }

    public async Task<Resultado<EmpresaResponse>> RegistrarAsync(RegistroEmpresaRequest request) {
        var rtn = request.Rtn.Trim();
        var email = ReglasIdentidad.NormalizarEmail(request.Email);
        var slug = SlugHelper.Generar(request.NombreComercial);

        if (slug.Length == 0) {
            return Resultado<EmpresaResponse>.Invalido(nameof(request.NombreComercial),
                "El nombre comercial debe incluir letras o números.");
        }

        if (await _empresas.ExisteRtnAsync(rtn)) {
            return Resultado<EmpresaResponse>.Conflicto("Registro duplicado", "El RTN ya está registrado.");
        }
        if (await _empresas.ExisteSlugAsync(slug)) {
            return Resultado<EmpresaResponse>.Conflicto("Registro duplicado",
                "Ya existe una empresa con un nombre comercial equivalente.");
        }
        if (await _usuarios.ExisteEmailAsync(email)) {
            return Resultado<EmpresaResponse>.Conflicto("Registro duplicado", "El email ya está registrado.");
        }

        var rolAdmin = await _roles.GetByNombreAsync(Roles.Admin)
            ?? throw new InvalidOperationException("El rol Admin no existe. Aplica las migraciones.");

        var empresa = new Empresa {
            Id = Guid.NewGuid(),
            NombreComercial = request.NombreComercial.Trim(),
            RazonSocial = request.RazonSocial.Trim(),
            Slug = slug,
            Rtn = rtn,
            Tipo = request.Tipo,
            Plan = request.Plan ?? PlanEmpresa.Basico,
            Activa = true
        };

        var admin = new Usuario {
            Id = Guid.NewGuid(),
            EmpresaId = empresa.Id,
            RolId = rolAdmin.Id,
            PrimerNombre = request.PrimerNombre.Trim(),
            SegundoNombre = Mapeos.LimpiarOpcional(request.SegundoNombre),
            PrimerApellido = request.PrimerApellido.Trim(),
            SegundoApellido = Mapeos.LimpiarOpcional(request.SegundoApellido),
            Telefono = Mapeos.LimpiarOpcional(request.Telefono),
            Email = email,
            PasswordHash = _hasher.Hash(request.Password),
            Activo = true
        };

        _empresas.Add(empresa);
        _usuarios.Add(admin);

        try {
            await _uow.GuardarAsync(); // un solo SaveChanges = una sola transaccion
        } catch (ConflictoException ex) {
            return ReglasIdentidad.MensajeConflicto<EmpresaResponse>(ex);
        }

        return Resultado<EmpresaResponse>.Ok(empresa.ToResponse());
    }
}
