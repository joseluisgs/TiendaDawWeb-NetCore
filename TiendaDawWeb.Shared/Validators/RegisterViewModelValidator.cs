using FluentValidation;
using TiendaDawWeb.Shared.ViewModels;

namespace TiendaDawWeb.Shared.Validators;

/// <summary>
/// Validador FluentValidation para el registro de usuarios.
/// Reglas de cascada: si un campo falla, los dependientes también.
/// </summary>
public class RegisterViewModelValidator : AbstractValidator<RegisterViewModel>
{
    public RegisterViewModelValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio")
            .MaximumLength(100).WithMessage("El nombre no puede tener más de 100 caracteres");

        RuleFor(x => x.Apellidos)
            .NotEmpty().WithMessage("Los apellidos son obligatorios")
            .MaximumLength(200).WithMessage("Los apellidos no pueden tener más de 200 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es obligatorio")
            .EmailAddress().WithMessage("Email no válido");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria")
            .MinimumLength(4).WithMessage("La contraseña debe tener al menos 4 caracteres")
            .MaximumLength(100).WithMessage("La contraseña no puede tener más de 100 caracteres");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Debes confirmar la contraseña")
            .Equal(x => x.Password).WithMessage("Las contraseñas no coinciden");
    }
}
