using System.Globalization;
using System.Text;

namespace quickgo.Application.Common;

public static class SlugHelper {
    public const int LongitudMaxima = 100;

    public static string Generar(string texto) {
        var normalizado = texto.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        var ultimoFueGuion = false;

        foreach (var c in normalizado) {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) {
                continue; // quita tildes
            }
            if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9')) {
                sb.Append(c);
                ultimoFueGuion = false;
            } else if (!ultimoFueGuion && sb.Length > 0) {
                sb.Append('-');
                ultimoFueGuion = true;
            }
        }

        var slug = sb.ToString().Trim('-');
        return slug.Length > LongitudMaxima ? slug[..LongitudMaxima].Trim('-') : slug;
    }
}
