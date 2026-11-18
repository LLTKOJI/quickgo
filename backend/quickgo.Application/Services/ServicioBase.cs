using quickgo.Application.Common;
using quickgo.Domain.Constants;
using quickgo.Domain.enums;

namespace quickgo.Application.Services;

internal static class ReglasIdentidad {
    public static string NormalizarEmail(string email) => email.Trim().ToLowerInvariant();

    /// <summary>Admin vale en cualquier empresa; Contratante solo en Contratante; Despachador y Conductor solo en Transportista.</summary>
    public static bool RolCompatible(string rol, TipoEmpresa tipo) => rol switch {
        Roles.Admin => true,
        Roles.Contratante => tipo == TipoEmpresa.Contratante,
        Roles.Despachador or Roles.Conductor => tipo == TipoEmpresa.Transportista,
        _ => false
    };

    public static Resultado<T> MensajeConflicto<T>(ConflictoException ex) {
        var r = ex.Restriccion ?? string.Empty;
        var detalle =
            r.Contains("rtn", StringComparison.OrdinalIgnoreCase) ? "El RTN ya está registrado." :
            r.Contains("slug", StringComparison.OrdinalIgnoreCase) ? "Ya existe una empresa con un nombre comercial equivalente." :
            r.Contains("email", StringComparison.OrdinalIgnoreCase) ? "El email ya está registrado." :
            r.Contains("licencia", StringComparison.OrdinalIgnoreCase) ? "La licencia ya está registrada en tu empresa." :
            "Ya existe un registro con esos datos.";
        return Resultado<T>.Conflicto("Registro duplicado", detalle);
    }
}
