using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using quickgo.Domain.Constants;

namespace quickgo.API.Security;

/// <summary>
/// Modo Dev (solo Development): permite probar sin login enviando los headers
/// X-Empresa-Id, X-User-Id, X-Rol y opcionalmente X-Tipo-Empresa.
/// </summary>
public class DevHeaderAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions> {
    public const string Scheme = "DevHeaders";

    public DevHeaderAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder) {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync() {
        var headers = Request.Headers;

        if (!Guid.TryParse(headers[DevHeaders.Empresa].ToString(), out var empresaId)) {
            return Task.FromResult(AuthenticateResult.Fail("X-Empresa-Id inválido."));
        }

        var rol = headers[DevHeaders.Rol].ToString();
        if (!Roles.Todos.Contains(rol)) {
            return Task.FromResult(AuthenticateResult.Fail("X-Rol debe ser Admin, Despachador, Conductor o Contratante."));
        }

        var userId = Guid.TryParse(headers[DevHeaders.Usuario].ToString(), out var u) ? u : Guid.Empty;
        var tipo = headers[DevHeaders.TipoEmpresa].ToString();
        if (string.IsNullOrWhiteSpace(tipo)) {
            tipo = rol == Roles.Contratante ? "Contratante" : "Transportista";
        }

        var identity = new ClaimsIdentity(new[] {
            new Claim(ClaimNames.UserId, userId.ToString()),
            new Claim(ClaimNames.EmpresaId, empresaId.ToString()),
            new Claim(ClaimNames.Rol, rol),
            new Claim(ClaimNames.TipoEmpresa, tipo)
        }, Scheme, ClaimNames.UserId, ClaimNames.Rol);

        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
