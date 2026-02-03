using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Diagnostics;
using TiendaDawWeb.Controllers;
using TiendaDawWeb.ViewModels;
using FluentAssertions;

namespace TiendaDawWeb.Tests.Controllers;

/// <summary>
/// OBJETIVO: Validar el comportamiento del controlador de errores.
/// LO QUE BUSCA: Asegurar que los errores HTTP se manejan correctamente.
/// </summary>
[TestFixture]
public class ErrorControllerTests
{
    private Mock<ILogger<ErrorController>> _loggerMock = null!;
    private ErrorController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<ErrorController>>();
        _controller = new ErrorController(_loggerMock.Object);

        var context = new DefaultHttpContext();
        context.TraceIdentifier = "test-trace-id";
        _controller.ControllerContext = new ControllerContext { HttpContext = context };
    }

    [TearDown]
    public void TearDown()
    {
        _controller?.Dispose();
    }

    /// <summary>
    /// PRUEBA: Error 404 muestra mensaje de página no encontrada.
    /// </summary>
    [Test]
    public void Index_ShouldReturn404Message_WhenStatusCodeIs404()
    {
        // Act
        var result = _controller.Index(404);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<ErrorViewModel>().Subject;
        model.StatusCode.Should().Be(404);
        model.Message.Should().Contain("no existe");
    }

    /// <summary>
    /// PRUEBA: Error 403 muestra mensaje de acceso denegado.
    /// </summary>
    [Test]
    public void Index_ShouldReturn403Message_WhenStatusCodeIs403()
    {
        // Act
        var result = _controller.Index(403);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<ErrorViewModel>().Subject;
        model.StatusCode.Should().Be(403);
        model.Message.Should().Contain("No tienes permisos");
    }

    /// <summary>
    /// PRUEBA: Error 401 muestra mensaje de sesión expirada.
    /// </summary>
    [Test]
    public void Index_ShouldReturn401Message_WhenStatusCodeIs401()
    {
        // Act
        var result = _controller.Index(401);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<ErrorViewModel>().Subject;
        model.StatusCode.Should().Be(401);
        model.Message.Should().Contain("identifícate");
    }

    /// <summary>
    /// PRUEBA: Error 500 muestra mensaje de error interno.
    /// </summary>
    [Test]
    public void Index_ShouldReturn500Message_WhenStatusCodeIs500()
    {
        // Act
        var result = _controller.Index(500);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<ErrorViewModel>().Subject;
        model.StatusCode.Should().Be(500);
        model.Message.Should().Contain("interno del servidor");
    }

    /// <summary>
    /// PRUEBA: Sin código de estado usa 500 por defecto.
    /// </summary>
    [Test]
    public void Index_ShouldUse500_WhenNoStatusCode()
    {
        // Act
        var result = _controller.Index();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<ErrorViewModel>().Subject;
        model.StatusCode.Should().Be(500);
        model.RequestId.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// PRUEBA: Error con código 200 se convierte a 500.
    /// </summary>
    [Test]
    public void Index_ShouldConvert200To500_WhenStatusCodeIs200()
    {
        // Act
        var result = _controller.Index(200);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<ErrorViewModel>().Subject;
        model.StatusCode.Should().Be(500);
    }

    /// <summary>
    /// PRUEBA: Error con código personalizado.
    /// </summary>
    [Test]
    public void Index_ShouldHandleCustomStatusCode()
    {
        // Act
        var result = _controller.Index(418); // I'm a teapot

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<ErrorViewModel>().Subject;
        model.StatusCode.Should().Be(418);
        model.Message.Should().Contain("error inesperado");
    }
}
