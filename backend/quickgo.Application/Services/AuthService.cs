using quickgo.Application.Common;
using quickgo.Application.Features.Identidad.DTOs;
using quickgo.Application.Interfaces;

namespace quickgo.Application.Services;

/// <summary>B1-01: inicio de sesion con JWT.</summary>
public class AuthService {
    // Mismo mensaje para correo inexistente o contraseña incorrecta.
    private const string MensajeCredenciales = "Correo o contraseña incorrectos.";

    private readonly IUsuarioRepository _usuarios;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokens;

    public AuthService(IUsuarioRepository usuarios, IPasswordHasher hasher, ITokenService tokens) {
        _usuarios = usuarios;
        _hasher = hasher;
        _tokens = tokens;
    }

    public async Task<Resultado<LoginResponse>> LoginAsync(LoginRequest request) {
        var email = ReglasIdentidad.NormalizarEmail(request.Email);
        var usuario = await _usuarios.GetParaLoginAsync(email);

        if (usuario is null) {
            _hasher.SimularVerificacion(request.Password);
            return Resultado<LoginResponse>.NoAutenticado("Credenciales inválidas", MensajeCredenciales);
        }

        if (!_hasher.Verify(usuario.PasswordHash, request.Password)) {
            return Resultado<LoginResponse>.NoAutenticado("Credenciales inválidas", MensajeCredenciales);
        }

        if (!usuario.Activo) {
            return Resultado<LoginResponse>.Prohibido("Usuario inactivo", "Tu usuario está inactivo.");
        }
        if (usuario.Empresa is null || !usuario.Empresa.Activa) {
            return Resultado<LoginResponse>.Prohibido("Empresa inactiva", "Tu empresa está inactiva.");
        }

        var rol = usuario.Rol?.Nombre ?? throw new InvalidOperationException("El usuario no tiene rol.");
        var token = _tokens.Generar(usuario.Id, usuario.EmpresaId, rol, usuario.Empresa.Tipo);

        return Resultado<LoginResponse>.Ok(new LoginResponse(
            token.Token, token.ExpiraEn, usuario.NombreCompleto, rol, usuario.Empresa.Tipo));
    }
}
