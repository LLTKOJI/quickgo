using FluentValidation;

namespace quickgo.Application.Features.Identidad.Validators;

public static class ReglasComunes {
    public static IRuleBuilderOptions<T, string> PasswordValida<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
            .Matches("[A-Za-z]").WithMessage("La contraseña debe incluir al menos una letra.")
            .Matches("[0-9]").WithMessage("La contraseña debe incluir al menos un número.");

    public static IRuleBuilderOptions<T, string> EmailValido<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage("El email es obligatorio.")
            .EmailAddress().WithMessage("El email no tiene un formato válido.")
            .MaximumLength(150).WithMessage("El email no puede tener más de 150 caracteres.");

    public static IRuleBuilderOptions<T, string> Nombre<T>(this IRuleBuilder<T, string> rule, string campo) =>
        rule.NotEmpty().WithMessage($"{campo} es obligatorio.")
            .MaximumLength(60).WithMessage($"{campo} no puede tener más de 60 caracteres.");

    public static IRuleBuilderOptions<T, string?> NombreOpcional<T>(this IRuleBuilder<T, string?> rule, string campo) =>
        rule.MaximumLength(60).WithMessage($"{campo} no puede tener más de 60 caracteres.");

    public static IRuleBuilderOptions<T, string?> TelefonoOpcional<T>(this IRuleBuilder<T, string?> rule) =>
        rule.MaximumLength(30).WithMessage("El teléfono no puede tener más de 30 caracteres.");
}
