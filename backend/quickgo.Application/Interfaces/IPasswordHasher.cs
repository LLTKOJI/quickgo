namespace quickgo.Application.Interfaces;

public interface IPasswordHasher {
    string Hash(string password);
    bool Verify(string hash, string password);

    /// <summary>Gasta el mismo tiempo que Verify (para no revelar si el correo existe).</summary>
    void SimularVerificacion(string password);
}
