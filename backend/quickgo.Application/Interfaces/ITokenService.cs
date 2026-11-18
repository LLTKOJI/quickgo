using quickgo.Domain.enums;

namespace quickgo.Application.Interfaces;

public record TokenGenerado(string Token, DateTimeOffset ExpiraEn);

public interface ITokenService {
    TokenGenerado Generar(Guid userId, Guid empresaId, string rol, TipoEmpresa tipoEmpresa);
}
