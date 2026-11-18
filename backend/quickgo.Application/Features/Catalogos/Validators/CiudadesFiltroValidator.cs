
using FluentValidation;
using quickgo.Application.Features.Catalogos.DTOs;
namespace quickgo.Application.Features.Catalogos.Validators;

public class CiudadesFiltroValidator : AbstractValidator<CiudadesFiltroDto> {
    public CiudadesFiltroValidator() {
        RuleFor(x => x.DepartamentoId)
            .NotEmpty().WithMessage("El departamentoId es obligatorio.");
    }
}