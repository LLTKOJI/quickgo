using FluentValidation;
using quickgo.Application.Features.Facturas.DTOs;
using quickgo.Domain.enums;

namespace quickgo.Application.Features.Facturas.Validators;

public class CreatePagoValidator : AbstractValidator<CreatePagoRequestDto> {
    public CreatePagoValidator() {
        RuleFor(x => x.Metodo)
            .Must(m => m == MetodosPago.Efectivo || m == MetodosPago.Transferencia || m == MetodosPago.Tarjeta)
            .WithMessage("El método debe ser Efectivo, Transferencia o Tarjeta.");

        RuleFor(x => x.Monto)
            .GreaterThan(0).WithMessage("El monto debe ser mayor que 0.");
    }
}