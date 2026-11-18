using FluentValidation;
using quickgo.Application.Features.Direcciones.DTOs;

namespace quickgo.Application.Features.Direcciones.Validators;

public class DireccionValidator : AbstractValidator<DireccionRequestDto> {
    public DireccionValidator() {
        RuleFor(x => x.CiudadId)
            .NotEmpty().WithMessage("La ciudad es obligatoria.");

        RuleFor(x => x.CallePrincipal)
            .NotEmpty().WithMessage("La calle principal es obligatoria.")
            .MaximumLength(150).WithMessage("La calle principal no puede tener más de 150 caracteres.");

        RuleFor(x => x.NumeroExterior)
            .MaximumLength(20).WithMessage("El número exterior no puede tener más de 20 caracteres.");

        RuleFor(x => x.ColoniaSector)
            .MaximumLength(100).WithMessage("La colonia o sector no puede tener más de 100 caracteres.");

        RuleFor(x => x.Referencia)
            .MaximumLength(250).WithMessage("La referencia no puede tener más de 250 caracteres.");

        RuleFor(x => x.Lat)
            .NotNull().WithMessage("La latitud es obligatoria.")
            .InclusiveBetween(-90m, 90m).WithMessage("La latitud debe estar entre -90 y 90.");

        RuleFor(x => x.Lng)
            .NotNull().WithMessage("La longitud es obligatoria.")
            .InclusiveBetween(-180m, 180m).WithMessage("La longitud debe estar entre -180 y 180.");
    }
}
