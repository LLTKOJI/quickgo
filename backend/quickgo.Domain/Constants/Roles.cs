namespace quickgo.Domain.Constants;

public static class Roles {
    public const string Admin = "Admin";
    public const string Despachador = "Despachador";
    public const string Conductor = "Conductor";
    public const string Contratante = "Contratante";

    // Combinaciones para [Authorize(Roles = ...)]
    public const string AdminDespachador = Admin + "," + Despachador;
    public const string AdminDespachadorConductor = Admin + "," + Despachador + "," + Conductor;

    public static readonly string[] Todos = { Admin, Despachador, Conductor, Contratante };
}
