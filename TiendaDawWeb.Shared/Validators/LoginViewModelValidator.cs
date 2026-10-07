using FluentValidation;
using TiendaDawWeb.Shared.ViewModels;

namespace TiendaDawWeb.Shared.Validators;

/// <summary>
/// Validador FluentValidation para el login.
/// </summary>
public class LoginViewModelValidator : AbstractValidator<LoginViewModel>
{
    public LoginViewModelValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es obligatorio")
            .EmailAddress().WithMessage("Email no válido");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria");
    }
}
