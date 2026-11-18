using System.Security.Cryptography;
using quickgo.Application.Interfaces;

namespace quickgo.Infrastructure.Security;

/// <summary>Hash de contraseñas con PBKDF2-SHA256 (sin paquetes externos). Formato: PBKDF2$iteraciones$sal$hash.</summary>
public class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int Iteraciones = 100_000;
    private const int LongitudSal = 16;
    private const int LongitudHash = 32;

    private static readonly string HashFalso = Generar("contraseña-falsa");

    public string Hash(string password) => Generar(password);

    public bool Verify(string hash, string password)
    {
        var partes = hash.Split('$');
        if (partes.Length != 4 || partes[0] != "PBKDF2" || !int.TryParse(partes[1], out var iteraciones))
        {
            return false;
        }

        try
        {
            var sal = Convert.FromBase64String(partes[2]);
            var esperado = Convert.FromBase64String(partes[3]);
            var actual = Rfc2898DeriveBytes.Pbkdf2(password, sal, iteraciones, HashAlgorithmName.SHA256, esperado.Length);
            return CryptographicOperations.FixedTimeEquals(actual, esperado);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public void SimularVerificacion(string password) => Verify(HashFalso, password);

    private static string Generar(string password)
    {
        var sal = RandomNumberGenerator.GetBytes(LongitudSal);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, sal, Iteraciones, HashAlgorithmName.SHA256, LongitudHash);
        return $"PBKDF2${Iteraciones}${Convert.ToBase64String(sal)}${Convert.ToBase64String(hash)}";
    }
}
