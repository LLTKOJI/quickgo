using FluentValidation;
using quickgo.Application.Features.Identidad.DTOs;
using quickgo.Domain.enums;

namespace quickgo.Application.Features.Identidad.Validators;

public class RegistroEmpresaValidator : AbstractValidator<RegistroEmpresaRequest> {
    public RegistroEmpresaValidator() {
        RuleFor(x => x.NombreComercial).NotEmpty().WithMessage("El nombre comercial es obligatorio.")
            .MaximumLength(150).WithMessage("El nombre comercial no puede tener más de 150 caracteres.");
        RuleFor(x => x.RazonSocial).NotEmpty().WithMessage("La razón social es obligatoria.")
            .MaximumLength(150).WithMessage("La razón social no puede tener más de 150 caracteres.");
        RuleFor(x => x.Rtn).NotEmpty().WithMessage("El RTN es obligatorio.")
            .MaximumLength(20).WithMessage("El RTN no puede tener más de 20 caracteres.");
        RuleFor(x => x.Tipo).IsInEnum().WithMessage("El tipo debe ser Transportista o Contratante.");
        RuleFor(x => x.Plan).IsInEnum().When(x => x.Plan.HasValue)
            .WithMessage("El plan debe ser Basico, Profesional o Empresarial.");

        RuleFor(x => x.PrimerNombre).Nombre("El primer nombre");
        RuleFor(x => x.SegundoNombre).NombreOpcional("El segundo nombre");
        RuleFor(x => x.PrimerApellido).Nombre("El primer apellido");
        RuleFor(x => x.SegundoApellido).NombreOpcional("El segundo apellido");
        RuleFor(x => x.Telefono).TelefonoOpcional();
        RuleFor(x => x.Email).EmailValido();
        RuleFor(x => x.Password).PasswordValida();
    }
}

public class LoginValidator : AbstractValidator<LoginRequest> {
    public LoginValidator() {
        RuleFor(x => x.Email).NotEmpty().WithMessage("El email es obligatorio.")
            .EmailAddress().WithMessage("El email no tiene un formato válido.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("La contraseña es obligatoria.");
    }
}

public class CrearUsuarioValidator : AbstractValidator<CrearUsuarioRequest> {
    public CrearUsuarioValidator() {
        RuleFor(x => x.PrimerNombre).Nombre("El primer nombre");
        RuleFor(x => x.SegundoNombre).NombreOpcional("El segundo nombre");
        RuleFor(x => x.PrimerApellido).Nombre("El primer apellido");
        RuleFor(x => x.SegundoApellido).NombreOpcional("El segundo apellido");
        RuleFor(x => x.Telefono).TelefonoOpcional();
        RuleFor(x => x.Email).EmailValido();
        RuleFor(x => x.Password).PasswordValida();
        RuleFor(x => x.RolId).NotEmpty().WithMessage("El rol es obligatorio.");
    }
}

public class ActualizarUsuarioValidator : AbstractValidator<ActualizarUsuarioRequest> {
    public ActualizarUsuarioValidator() {
        RuleFor(x => x.PrimerNombre).Nombre("El primer nombre");
        RuleFor(x => x.SegundoNombre).NombreOpcional("El segundo nombre");
        RuleFor(x => x.PrimerApellido).Nombre("El primer apellido");
        RuleFor(x => x.SegundoApellido).NombreOpcional("El segundo apellido");
        RuleFor(x => x.Telefono).TelefonoOpcional();
        RuleFor(x => x.RolId).NotEmpty().WithMessage("El rol es obligatorio.");
    }
}

public class CrearConductorValidator : AbstractValidator<CrearConductorRequest> {
    public CrearConductorValidator() {
        RuleFor(x => x.PrimerNombre).Nombre("El primer nombre");
        RuleFor(x => x.SegundoNombre).NombreOpcional("El segundo nombre");
        RuleFor(x => x.PrimerApellido).Nombre("El primer apellido");
        RuleFor(x => x.SegundoApellido).NombreOpcional("El segundo apellido");
        RuleFor(x => x.Telefono).TelefonoOpcional();
        RuleFor(x => x.TipoConductor).IsInEnum().WithMessage("El tipo de conductor debe ser Interno o Externo.");

        RuleFor(x => x.Licencia).NotEmpty().WithMessage("La licencia es obligatoria.")
            .MaximumLength(50).WithMessage("La licencia no puede tener más de 50 caracteres.");
        RuleFor(x => x.VenceLicencia).NotEqual(default(DateOnly))
            .WithMessage("La fecha de vencimiento de la licencia es obligatoria.");

        // Interno: necesita acceso al sistema. Externo: Email y Password se ignoran.
        When(x => x.TipoConductor == TipoConductor.Interno, () => {
            RuleFor(x => x.Email!).EmailValido();
            RuleFor(x => x.Password!).PasswordValida();
        });
    }
}

public class ActualizarConductorValidator : AbstractValidator<ActualizarConductorRequest> {
    public ActualizarConductorValidator() {
        RuleFor(x => x.PrimerNombre).Nombre("El primer nombre");
        RuleFor(x => x.SegundoNombre).NombreOpcional("El segundo nombre");
        RuleFor(x => x.PrimerApellido).Nombre("El primer apellido");
        RuleFor(x => x.SegundoApellido).NombreOpcional("El segundo apellido");
        RuleFor(x => x.Telefono).TelefonoOpcional();
        RuleFor(x => x.Licencia).NotEmpty().WithMessage("La licencia es obligatoria.")
            .MaximumLength(50).WithMessage("La licencia no puede tener más de 50 caracteres.");
        RuleFor(x => x.VenceLicencia).NotEqual(default(DateOnly))
            .WithMessage("La fecha de vencimiento de la licencia es obligatoria.");
        RuleFor(x => x.Estado).IsInEnum().WithMessage("El estado debe ser Activo o Inactivo.");
    }
}

public class LicenciasPorVencerValidator : AbstractValidator<LicenciasPorVencerQuery> {
    public LicenciasPorVencerValidator() {
        RuleFor(x => x.Dias).InclusiveBetween(1, 365).WithMessage("Los días deben estar entre 1 y 365.");
    }
}
