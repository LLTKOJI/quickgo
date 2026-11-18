using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using quickgo.Application.Interfaces;
using quickgo.Domain.enums;

namespace quickgo.API.Security;

public class JwtTokenService : ITokenService {
    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options) {
        _options = options.Value;
    }

    public TokenGenerado Generar(Guid userId, Guid empresaId, string rol, TipoEmpresa tipoEmpresa) {
        var expira = DateTimeOffset.UtcNow.AddMinutes(_options.ExpiraMinutos);

        var descriptor = new SecurityTokenDescriptor {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Expires = expira.UtcDateTime,
            Subject = new ClaimsIdentity(new[] {
                new Claim(ClaimNames.UserId, userId.ToString()),
                new Claim(ClaimNames.EmpresaId, empresaId.ToString()),
                new Claim(ClaimNames.Rol, rol),
                new Claim(ClaimNames.TipoEmpresa, tipoEmpresa.ToString())
            }),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key)),
                SecurityAlgorithms.HmacSha256)
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);
        return new TokenGenerado(token, expira);
    }
}
