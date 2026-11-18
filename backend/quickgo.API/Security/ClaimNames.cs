namespace quickgo.API.Security;

/// <summary>Nombres de los claims del JWT (y de los headers del modo Dev).</summary>
public static class ClaimNames {
    public const string UserId = "userId";
    public const string EmpresaId = "empresaId";
    public const string Rol = "rol";
    public const string TipoEmpresa = "tipoEmpresa";
}

public static class DevHeaders {
    public const string Empresa = "X-Empresa-Id";
    public const string Usuario = "X-User-Id";
    public const string Rol = "X-Rol";
    public const string TipoEmpresa = "X-Tipo-Empresa";
}
