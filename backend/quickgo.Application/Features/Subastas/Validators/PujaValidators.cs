using FluentValidation;
using quickgo.Application.Features.Subastas.DTOs;

namespace quickgo.Application.Features.Subastas.Validators;

public class CrearPujaValidator : AbstractValidator<CrearPujaRequest>
{
    public CrearPujaValidator()
    {
        RuleFor(x => x.ConductorId).NotEmpty();
        RuleFor(x => x.Monto).GreaterThan(0);
        RuleFor(x => x.Mensaje).MaximumLength(500);
    }
}

public class AdjudicarValidator : AbstractValidator<AdjudicarRequest>
{
    public AdjudicarValidator()
    {
        RuleFor(x => x.PujaId).NotEmpty();
    }
}
