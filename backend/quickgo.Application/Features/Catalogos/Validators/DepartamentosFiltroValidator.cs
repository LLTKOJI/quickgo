
using FluentValidation;
using quickgo.Application.Features.Catalogos.DTOs;
namespace quickgo.Application.Features.Catalogos.Validators;

public class DepartamentosFiltroValidator : AbstractValidator<DepartamentosFiltroDto> {
    public DepartamentosFiltroValidator() {
        RuleFor(x => x.PaisId)
            .NotEmpty().WithMessage("El paisId es obligatorio.");
    }
}
