using FluentAssertions;
using TiendaDawWeb.Shared.ViewModels;

namespace TiendaDawWeb.Tests.Shared.ViewModels;

public class LoginViewModelTests
{
    [Test]
    public void LoginViewModel_CanSetEmail()
    {
        var model = new LoginViewModel { Email = "test@example.com" };

        model.Email.Should().Be("test@example.com");
    }

    [Test]
    public void LoginViewModel_CanSetPassword()
    {
        var model = new LoginViewModel { Password = "password123" };

        model.Password.Should().Be("password123");
    }

    [Test]
    public void LoginViewModel_DefaultRememberMe_IsFalse()
    {
        var model = new LoginViewModel();

        model.RememberMe.Should().BeFalse();
    }

    [Test]
    public void LoginViewModel_CanSetRememberMe()
    {
        var model = new LoginViewModel { RememberMe = true };

        model.RememberMe.Should().BeTrue();
    }

    [Test]
    public void LoginViewModel_CanSetReturnUrl()
    {
        var model = new LoginViewModel { ReturnUrl = "/home" };

        model.ReturnUrl.Should().Be("/home");
    }

    [Test]
    public void LoginViewModel_CanHaveNullReturnUrl()
    {
        var model = new LoginViewModel { ReturnUrl = null };

        model.ReturnUrl.Should().BeNull();
    }

    [Test]
    public void LoginViewModel_CanSetAllProperties()
    {
        var model = new LoginViewModel
        {
            Email = "user@example.com",
            Password = "secret123",
            RememberMe = true,
            ReturnUrl = "/dashboard"
        };

        model.Email.Should().Be("user@example.com");
        model.Password.Should().Be("secret123");
        model.RememberMe.Should().BeTrue();
        model.ReturnUrl.Should().Be("/dashboard");
    }

    [Test]
    public void LoginViewModel_DefaultValues()
    {
        var model = new LoginViewModel();

        model.Email.Should().Be(string.Empty);
        model.Password.Should().Be(string.Empty);
        model.RememberMe.Should().BeFalse();
        model.ReturnUrl.Should().BeNull();
    }
}

public class ErrorViewModelTests
{
    [Test]
    public void ErrorViewModel_CanSetRequestId()
    {
        var model = new ErrorViewModel { RequestId = "abc123" };

        model.RequestId.Should().Be("abc123");
    }

    [Test]
    public void ErrorViewModel_CanSetStatusCode()
    {
        var model = new ErrorViewModel { StatusCode = 404 };

        model.StatusCode.Should().Be(404);
    }

    [Test]
    public void ErrorViewModel_CanSetMessage()
    {
        var model = new ErrorViewModel { Message = "Something went wrong" };

        model.Message.Should().Be("Something went wrong");
    }

    [Test]
    public void ErrorViewModel_ShowRequestId_IsFalse_WhenNull()
    {
        var model = new ErrorViewModel { RequestId = null };

        model.ShowRequestId.Should().BeFalse();
    }

    [Test]
    public void ErrorViewModel_ShowRequestId_IsFalse_WhenEmpty()
    {
        var model = new ErrorViewModel { RequestId = string.Empty };

        model.ShowRequestId.Should().BeFalse();
    }

    [Test]
    public void ErrorViewModel_ShowRequestId_IsTrue_WhenHasValue()
    {
        var model = new ErrorViewModel { RequestId = "xyz789" };

        model.ShowRequestId.Should().BeTrue();
    }

    [Test]
    public void ErrorViewModel_CanHaveNullStatusCode()
    {
        var model = new ErrorViewModel { StatusCode = null };

        model.StatusCode.Should().BeNull();
    }

    [Test]
    public void ErrorViewModel_CanHaveNullMessage()
    {
        var model = new ErrorViewModel { Message = null };

        model.Message.Should().BeNull();
    }

    [Test]
    public void ErrorViewModel_CanSetAllProperties()
    {
        var model = new ErrorViewModel
        {
            RequestId = "error123",
            StatusCode = 500,
            Message = "Internal server error"
        };

        model.RequestId.Should().Be("error123");
        model.StatusCode.Should().Be(500);
        model.Message.Should().Be("Internal server error");
        model.ShowRequestId.Should().BeTrue();
    }

    [Test]
    public void ErrorViewModel_DefaultValues()
    {
        var model = new ErrorViewModel();

        model.RequestId.Should().BeNull();
        model.StatusCode.Should().BeNull();
        model.Message.Should().BeNull();
        model.ShowRequestId.Should().BeFalse();
    }
}

public class AdminDashboardViewModelTests
{
    [Test]
    public void AdminDashboardViewModel_CanSetTotalStats()
    {
        var model = new AdminDashboardViewModel
        {
            TotalUsuarios = 100,
            TotalProductos = 500,
            TotalCompras = 200,
            TotalVentas = 50000.50m
        };

        model.TotalUsuarios.Should().Be(100);
        model.TotalProductos.Should().Be(500);
        model.TotalCompras.Should().Be(200);
        model.TotalVentas.Should().Be(50000.50m);
    }

    [Test]
    public void AdminDashboardViewModel_CanSetActiveStats()
    {
        var model = new AdminDashboardViewModel
        {
            UsuariosActivos = 50,
            ProductosDisponibles = 300,
            ComprasHoy = 10,
            ComprasSemana = 50,
            ComprasMes = 150
        };

        model.UsuariosActivos.Should().Be(50);
        model.ProductosDisponibles.Should().Be(300);
        model.ComprasHoy.Should().Be(10);
        model.ComprasSemana.Should().Be(50);
        model.ComprasMes.Should().Be(150);
    }

    [Test]
    public void AdminDashboardViewModel_CanSetSalesStats()
    {
        var model = new AdminDashboardViewModel
        {
            VentasHoy = 1500.00m,
            VentasSemana = 7500.00m,
            VentasMes = 25000.00m
        };

        model.VentasHoy.Should().Be(1500.00m);
        model.VentasSemana.Should().Be(7500.00m);
        model.VentasMes.Should().Be(25000.00m);
    }

    [Test]
    public void AdminDashboardViewModel_DefaultValues_AreZero()
    {
        var model = new AdminDashboardViewModel();

        model.TotalUsuarios.Should().Be(0);
        model.TotalProductos.Should().Be(0);
        model.TotalCompras.Should().Be(0);
        model.TotalVentas.Should().Be(0);
        model.UsuariosActivos.Should().Be(0);
        model.ProductosDisponibles.Should().Be(0);
        model.ComprasHoy.Should().Be(0);
        model.ComprasSemana.Should().Be(0);
        model.ComprasMes.Should().Be(0);
        model.VentasHoy.Should().Be(0);
        model.VentasSemana.Should().Be(0);
        model.VentasMes.Should().Be(0);
    }

    [Test]
    public void AdminDashboardViewModel_CanHandleDecimalValues()
    {
        var model = new AdminDashboardViewModel
        {
            TotalVentas = 1234567.89m,
            VentasHoy = 999.99m,
            VentasSemana = 9999.99m,
            VentasMes = 99999.99m
        };

        model.TotalVentas.Should().Be(1234567.89m);
        model.VentasHoy.Should().Be(999.99m);
        model.VentasSemana.Should().Be(9999.99m);
        model.VentasMes.Should().Be(99999.99m);
    }

    [Test]
    public void AdminDashboardViewModel_CanHandleNegativeValues()
    {
        var model = new AdminDashboardViewModel
        {
            TotalVentas = -100.00m
        };

        model.TotalVentas.Should().Be(-100.00m);
    }

    [Test]
    public void AdminDashboardViewModel_CanHandleLargeNumbers()
    {
        var model = new AdminDashboardViewModel
        {
            TotalUsuarios = int.MaxValue,
            TotalVentas = decimal.MaxValue
        };

        model.TotalUsuarios.Should().Be(int.MaxValue);
        model.TotalVentas.Should().Be(decimal.MaxValue);
    }
}

public class RegisterViewModelTests
{
    [Test]
    public void RegisterViewModel_CanSetNombre()
    {
        var model = new RegisterViewModel { Nombre = "John" };
        model.Nombre.Should().Be("John");
    }

    [Test]
    public void RegisterViewModel_CanSetApellidos()
    {
        var model = new RegisterViewModel { Apellidos = "Doe" };
        model.Apellidos.Should().Be("Doe");
    }

    [Test]
    public void RegisterViewModel_CanSetEmail()
    {
        var model = new RegisterViewModel { Email = "john@example.com" };
        model.Email.Should().Be("john@example.com");
    }

    [Test]
    public void RegisterViewModel_CanSetPassword()
    {
        var model = new RegisterViewModel { Password = "password123" };
        model.Password.Should().Be("password123");
    }

    [Test]
    public void RegisterViewModel_CanSetConfirmPassword()
    {
        var model = new RegisterViewModel { ConfirmPassword = "password123" };
        model.ConfirmPassword.Should().Be("password123");
    }

    [Test]
    public void RegisterViewModel_CanSetAvatar()
    {
        var model = new RegisterViewModel { Avatar = "https://example.com/avatar.jpg" };
        model.Avatar.Should().Be("https://example.com/avatar.jpg");
    }

    [Test]
    public void RegisterViewModel_CanHaveNullAvatar()
    {
        var model = new RegisterViewModel { Avatar = null };
        model.Avatar.Should().BeNull();
    }

    [Test]
    public void RegisterViewModel_DefaultNombre_IsEmpty()
    {
        var model = new RegisterViewModel();
        model.Nombre.Should().Be(string.Empty);
    }

    [Test]
    public void RegisterViewModel_DefaultEmail_IsEmpty()
    {
        var model = new RegisterViewModel();
        model.Email.Should().Be(string.Empty);
    }

    [Test]
    public void RegisterViewModel_DefaultPassword_IsEmpty()
    {
        var model = new RegisterViewModel();
        model.Password.Should().Be(string.Empty);
    }

    [Test]
    public void RegisterViewModel_CanSetAllProperties()
    {
        var model = new RegisterViewModel
        {
            Nombre = "John",
            Apellidos = "Doe",
            Email = "john@example.com",
            Password = "password123",
            ConfirmPassword = "password123",
            Avatar = "https://example.com/avatar.jpg"
        };

        model.Nombre.Should().Be("John");
        model.Apellidos.Should().Be("Doe");
        model.Email.Should().Be("john@example.com");
        model.Password.Should().Be("password123");
        model.ConfirmPassword.Should().Be("password123");
        model.Avatar.Should().Be("https://example.com/avatar.jpg");
    }
}
