using FluentValidation;
using quickgo.Application.Features.Envios.DTOs;
using quickgo.Domain.Enums;

namespace quickgo.Application.Features.Envios.Validators;

public class CrearEnvioValidator : AbstractValidator<CrearEnvioRequest>
{
    public CrearEnvioValidator()
    {
        RuleFor(x => x.ClienteId).NotEmpty();
        RuleFor(x => x.OrigenDireccionId).NotEmpty();
        RuleFor(x => x.DestinoDireccionId).NotEmpty()
            .Must((r, d) => d != r.OrigenDireccionId).WithMessage("El origen y el destino deben ser distintos.");
        RuleFor(x => x.TipoServicio).IsInEnum().Must(t => t is TipoServicio.Carga or TipoServicio.Grua or TipoServicio.Maquinaria)
            .WithMessage("Tipo de servicio inválido.");
        RuleFor(x => x.FechaProgramada).Must(f => f >= DateTimeOffset.UtcNow.AddMinutes(-1))
            .WithMessage("La fecha programada no puede estar en el pasado.");
        RuleFor(x => x.PesoKg).GreaterThan(0);
        RuleFor(x => x.VolumenM3).GreaterThan(0).When(x => x.VolumenM3.HasValue);
        RuleFor(x => x.Tarifa).GreaterThanOrEqualTo(0);
    }
}

public class ActualizarEnvioValidator : AbstractValidator<ActualizarEnvioRequest>
{
    public ActualizarEnvioValidator()
    {
        Include(new CrearEnvioValidator());
    }
}

public class AsignarEnvioValidator : AbstractValidator<AsignarEnvioRequest>
{
    public AsignarEnvioValidator()
    {
        RuleFor(x => x.VehiculoId).NotEmpty();
        RuleFor(x => x.ConductorId).NotEmpty();
        RuleFor(x => x.CostoExterno).GreaterThanOrEqualTo(0).When(x => x.CostoExterno.HasValue);
    }
}

public class CambiarEstadoValidator : AbstractValidator<CambiarEstadoRequest>
{
    public CambiarEstadoValidator()
    {
        RuleFor(x => x.Estado).IsInEnum();
    }
}

public class CrearCotizacionValidator : AbstractValidator<CrearCotizacionRequest>
{
    public CrearCotizacionValidator()
    {
        RuleFor(x => x.Monto).GreaterThan(0);
        RuleFor(x => x.MonedaCodigo).NotEmpty().Length(3);
        RuleFor(x => x.ValidaHasta).Must(v => v > DateTimeOffset.UtcNow)
            .WithMessage("La vigencia debe estar en el futuro.");
    }
}

public class RegistrarSeguimientoValidator : AbstractValidator<RegistrarSeguimientoRequest>
{
    public RegistrarSeguimientoValidator()
    {
        RuleFor(x => x.Lat).InclusiveBetween(-90, 90);
        RuleFor(x => x.Lng).InclusiveBetween(-180, 180);
        RuleFor(x => x.Estado).NotEmpty().MaximumLength(30);
    }
}
