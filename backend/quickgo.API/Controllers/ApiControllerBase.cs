using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using quickgo.Application.Common;

namespace quickgo.API.Controllers;

/// <summary>Formato unico de errores (ProblemDetails) para los controladores del modulo de Identidad.</summary>
public abstract class ApiControllerBase : ControllerBase {

    /// <summary>Devuelve un 400 si el modelo no es valido; null si todo esta bien.</summary>
    protected async Task<IActionResult?> ValidarAsync<T>(IValidator<T> validator, T modelo) {
        var resultado = await validator.ValidateAsync(modelo);
        if (resultado.IsValid) {
            return null;
        }
        return ValidationProblem(new ValidationProblemDetails(resultado.ToDictionary()));
    }

    protected IActionResult Responder<T>(Resultado<T> resultado, Func<T, IActionResult>? ok = null) {
        if (resultado.Exito) {
            return ok is null ? Ok(resultado.Valor) : ok(resultado.Valor!);
        }

        return resultado.Error switch {
            TipoError.Validacion => ValidationProblem(new ValidationProblemDetails(
                resultado.Errores ?? new Dictionary<string, string[]>()) { Detail = resultado.Detalle }),
            TipoError.NoAutenticado => Problem(statusCode: 401, title: resultado.Titulo, detail: resultado.Detalle),
            TipoError.Prohibido => Problem(statusCode: 403, title: resultado.Titulo, detail: resultado.Detalle),
            TipoError.NoEncontrado => Problem(statusCode: 404, title: resultado.Titulo, detail: resultado.Detalle),
            TipoError.Conflicto => Problem(statusCode: 409, title: resultado.Titulo, detail: resultado.Detalle),
            _ => Problem(statusCode: 500, title: "Error inesperado")
        };
    }

    protected IActionResult Creado<T>(Resultado<T> resultado, Func<T, string> ubicacion) =>
        Responder(resultado, valor => Created(ubicacion(valor), valor));
}
