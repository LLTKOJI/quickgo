using FluentValidation;
using quickgo.Application.Features.Facturas.DTOs;

namespace quickgo.Application.Features.Facturas.Validators;

public class CreateFacturaValidator : AbstractValidator<CreateFacturaRequestDto> {
    public CreateFacturaValidator() {
        RuleFor(x => x.EnvioId)
            .NotNull().NotEqual(Guid.Empty)
            .WithMessage("El envío es obligatorio.");

        RuleFor(x => x.MonedaCodigo)
            .Length(3).When(x => !string.IsNullOrWhiteSpace(x.MonedaCodigo))
            .WithMessage("El código de moneda debe tener 3 letras.");
    }
}
