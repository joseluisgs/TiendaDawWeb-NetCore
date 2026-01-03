using TiendaDawWeb.ViewModels;
using FluentAssertions;

namespace TiendaDawWeb.Tests;

/// <summary>
/// OBJETIVO: Validar la integridad de los ViewModels.
/// LO QUE BUSCA: Asegurar que las propiedades almacenan y recuperan datos correctamente,
/// y que la lógica simple de visualización (como ShowRequestId) funciona.
/// </summary>
[TestFixture]
public class ViewModelTests
{
    /// <summary>
    /// PRUEBA: Propiedades de LoginViewModel.
    /// OBJETIVO: Verificar el almacenamiento de credenciales y URL de retorno.
    /// </summary>
    [Test]
    public void LoginViewModel_ShouldStoreProperties()
    {
        // Arrange & Act
        var model = new LoginViewModel
        {
            Email = "test@test.com",
            Password = "password",
            RememberMe = true,
            ReturnUrl = "/home"
        };

        // Assert
        model.Email.Should().Be("test@test.com");
        model.Password.Should().Be("password");
        model.RememberMe.Should().BeTrue();
        model.ReturnUrl.Should().Be("/home");
    }

    /// <summary>
    /// PRUEBA: Lógica de visualización en ErrorViewModel.
    /// OBJETIVO: Validar que ShowRequestId depende de la presencia de un ID de solicitud.
    /// </summary>
    [Test]
    public void ErrorViewModel_ShouldShowRequestId_WhenNotNull()
    {
        // Arrange
        var model = new ErrorViewModel { RequestId = "123" };
        var modelNoId = new ErrorViewModel { RequestId = null };

        // Assert
        model.ShowRequestId.Should().BeTrue();
        modelNoId.ShowRequestId.Should().BeFalse();
    }

    /// <summary>
    /// PRUEBA: Propiedades de AdminDashboardViewModel.
    /// OBJETIVO: Confirmar que el dashboard almacena correctamente las métricas de administración.
    /// </summary>
    [Test]
    public void AdminDashboardViewModel_ShouldStoreProperties()
    {
        // Arrange & Act
        var model = new AdminDashboardViewModel
        {
            TotalUsuarios = 10,
            TotalProductos = 50,
            TotalCompras = 5,
            TotalVentas = 500.50m
        };

        // Assert
        model.TotalUsuarios.Should().Be(10);
        model.TotalProductos.Should().Be(50);
        model.TotalCompras.Should().Be(5);
        model.TotalVentas.Should().Be(500.50m);
    }
}
