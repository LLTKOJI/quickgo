namespace quickgo.Application.Common;

/// <summary>
/// Se lanza cuando la base rechaza un valor duplicado (indice unico).
/// Restriccion trae el nombre del indice violado.
/// </summary>
public class ConflictoException : Exception {
    public string? Restriccion { get; }

    public ConflictoException(string? restriccion) : base("Valor duplicado: " + restriccion) {
        Restriccion = restriccion;
    }
}
