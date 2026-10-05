using FluentAssertions;
using FluentValidation.Results;
using TiendaDawWeb.Shared.Validators;
using TiendaDawWeb.Shared.ViewModels;

namespace TiendaDawWeb.Tests.Validators;

/// <summary>
/// Tests unitarios para los validadores FluentValidation.
/// </summary>
public class ValidatorsTests
{
    [Test]
    public void RegisterValidator_ValidData_Passes()
    {
        var validator = new RegisterViewModelValidator();
        var model = new RegisterViewModel
        {
            Nombre = "Juan",
            Apellidos = "García",
            Email = "juan@test.com",
            Password = "1234",
            ConfirmPassword = "1234"
        };

        var result = validator.Validate(model);
        result.IsValid.Should().BeTrue();
    }

    [Test]
    public void RegisterValidator_EmptyNombre_Fails()
    {
        var validator = new RegisterViewModelValidator();
        var model = new RegisterViewModel
        {
            Nombre = "",
            Apellidos = "García",
            Email = "juan@test.com",
            Password = "1234",
            ConfirmPassword = "1234"
        };

        var result = validator.Validate(model);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Nombre");
    }

    [Test]
    public void RegisterValidator_PasswordMismatch_Fails()
    {
        var validator = new RegisterViewModelValidator();
        var model = new RegisterViewModel
        {
            Nombre = "Juan",
            Apellidos = "García",
            Email = "juan@test.com",
            Password = "1234",
            ConfirmPassword = "5678"
        };

        var result = validator.Validate(model);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ConfirmPassword");
    }

    [Test]
    public void LoginValidator_EmptyEmail_Fails()
    {
        var validator = new LoginViewModelValidator();
        var model = new LoginViewModel { Email = "", Password = "1234" };

        var result = validator.Validate(model);
        result.IsValid.Should().BeFalse();
    }

    [Test]
    public void LoginValidator_ValidData_Passes()
    {
        var validator = new LoginViewModelValidator();
        var model = new LoginViewModel { Email = "test@test.com", Password = "1234" };

        var result = validator.Validate(model);
        result.IsValid.Should().BeTrue();
    }
}
