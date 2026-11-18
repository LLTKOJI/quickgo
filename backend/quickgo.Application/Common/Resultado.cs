namespace quickgo.Application.Common;

public enum TipoError {
    Validacion = 1,
    NoAutenticado,
    Prohibido,
    NoEncontrado,
    Conflicto
}

public class Resultado {
    public TipoError? Error { get; init; }
    public string? Titulo { get; init; }
    public string? Detalle { get; init; }
    public IDictionary<string, string[]>? Errores { get; init; }
    public bool Exito => Error is null;
}

public class Resultado<T> : Resultado {
    public T? Valor { get; init; }

    public static Resultado<T> Ok(T valor) => new() { Valor = valor };

    public static Resultado<T> Invalido(string campo, string mensaje) => new() {
        Error = TipoError.Validacion,
        Titulo = "Error de validación",
        Detalle = mensaje,
        Errores = new Dictionary<string, string[]> { [campo] = new[] { mensaje } }
    };

    public static Resultado<T> NoAutenticado(string titulo, string detalle) =>
        new() { Error = TipoError.NoAutenticado, Titulo = titulo, Detalle = detalle };

    public static Resultado<T> Prohibido(string titulo, string detalle) =>
        new() { Error = TipoError.Prohibido, Titulo = titulo, Detalle = detalle };

    public static Resultado<T> NoEncontrado(string titulo, string detalle) =>
        new() { Error = TipoError.NoEncontrado, Titulo = titulo, Detalle = detalle };

    public static Resultado<T> Conflicto(string titulo, string detalle) =>
        new() { Error = TipoError.Conflicto, Titulo = titulo, Detalle = detalle };
}
