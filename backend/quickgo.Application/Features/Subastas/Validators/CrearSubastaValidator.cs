using FluentValidation;
using quickgo.Application.Features.Subastas.DTOs;

namespace quickgo.Application.Features.Subastas.Validators;

public class CrearSubastaValidator : AbstractValidator<CrearSubastaRequestDto> {
    public CrearSubastaValidator() {
        RuleFor(x => x.TipoVehiculoId).NotEmpty().WithMessage("El tipo de vehículo es obligatorio.");
        RuleFor(x => x.OrigenDireccionId).NotEmpty().WithMessage("La dirección de origen es obligatoria.");
        RuleFor(x => x.DestinoDireccionId).NotEmpty().WithMessage("La dirección de destino es obligatoria.");
        RuleFor(x => x.DestinoDireccionId)
            .NotEqual(x => x.OrigenDireccionId)
            .WithMessage("El origen y el destino deben ser distintos.");
        RuleFor(x => x.Descripcion).MaximumLength(500);
        RuleFor(x => x.PesoKg).GreaterThan(0).WithMessage("El peso debe ser mayor a 0.");
        RuleFor(x => x.VolumenM3).GreaterThan(0).When(x => x.VolumenM3.HasValue);
        RuleFor(x => x.PrecioMaximo).GreaterThan(0).WithMessage("El precio máximo debe ser mayor a 0.");
        RuleFor(x => x.MonedaCodigo).NotEmpty().Length(3).WithMessage("La moneda debe tener 3 letras.");
        RuleFor(x => x.FechaCierre)
            .Must(f => f > DateTimeOffset.UtcNow)
            .WithMessage("La fecha de cierre debe estar en el futuro.");
    }
}