using FluentValidation;
using quickgo.Application.Features.Clientes.DTOs;
using quickgo.Domain.enums;

namespace quickgo.Application.Features.Clientes.Validators;

public class ClienteValidator : AbstractValidator<ClienteRequestDto> {
    public ClienteValidator() {
        RuleFor(x => x.PrimerNombre)
            .NotEmpty().WithMessage("El primer nombre es obligatorio.")
            .MaximumLength(60).WithMessage("El primer nombre no puede tener más de 60 caracteres.");

        RuleFor(x => x.SegundoNombre)
            .MaximumLength(60).WithMessage("El segundo nombre no puede tener más de 60 caracteres.");

        RuleFor(x => x.PrimerApellido)
            .NotEmpty().WithMessage("El primer apellido es obligatorio.")
            .MaximumLength(60).WithMessage("El primer apellido no puede tener más de 60 caracteres.");

        RuleFor(x => x.SegundoApellido)
            .MaximumLength(60).WithMessage("El segundo apellido no puede tener más de 60 caracteres.");

        RuleFor(x => x.Telefono)
            .NotEmpty().WithMessage("El teléfono es obligatorio.")
            .MaximumLength(30).WithMessage("El teléfono no puede tener más de 30 caracteres.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("El email no tiene un formato válido.")
            .MaximumLength(150).WithMessage("El email no puede tener más de 150 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.TipoCliente)
            .Must(t => t == TiposCliente.Empresa || t == TiposCliente.Particular)
            .WithMessage("El tipo de cliente debe ser Empresa o Particular.");
    }
}
