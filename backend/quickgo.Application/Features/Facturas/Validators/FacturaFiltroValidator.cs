using FluentValidation;
using quickgo.Application.Features.Facturas.DTOs;
using quickgo.Domain.enums;
namespace quickgo.Application.Features.Facturas.Validators;


public class FacturaFiltroValidator : AbstractValidator<FacturaFiltroDto> {
    public FacturaFiltroValidator() {
        RuleFor(x => x.Estado)
            .Must(e => e == EstadosFactura.Pendiente || e == EstadosFactura.Pagada)
            .When(x => !string.IsNullOrWhiteSpace(x.Estado))
            .WithMessage("El estado debe ser Pendiente o Pagada.");
    }
}