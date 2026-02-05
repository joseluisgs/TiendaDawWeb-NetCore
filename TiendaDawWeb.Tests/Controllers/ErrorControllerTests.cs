using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using TiendaDawWeb.Controllers;
using TiendaDawWeb.Shared.ViewModels;

namespace TiendaDawWeb.Tests.ErrorHandling;

public class ErrorHandlingTests
{
    [Test]
    public void ErrorController_Index_With404_ShouldReturnErrorView()
    {
        var loggerMock = new Mock<ILogger<ErrorController>>();
        var controller = new ErrorController(loggerMock.Object);

        var httpContext = new DefaultHttpContext();
        httpContext.Response.StatusCode = 404;
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var result = controller.Index(404);

        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = (ViewResult)result;
        Assert.That(viewResult.Model, Is.InstanceOf<ErrorViewModel>());

        var errorModel = (ErrorViewModel)viewResult.Model!;
        Assert.That(errorModel.StatusCode, Is.EqualTo(404));
        Assert.That(errorModel.Message, Does.Contain("no existe"));
        Assert.That(errorModel.ShowRequestId, Is.True);
    }

    [Test]
    public void ErrorController_Index_With500_ShouldReturnErrorView()
    {
        var loggerMock = new Mock<ILogger<ErrorController>>();
        var controller = new ErrorController(loggerMock.Object);

        var httpContext = new DefaultHttpContext();
        httpContext.Response.StatusCode = 500;
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var result = controller.Index(500);

        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = (ViewResult)result;
        var errorModel = (ErrorViewModel)viewResult.Model!;
        Assert.That(errorModel.StatusCode, Is.EqualTo(500));
        Assert.That(errorModel.Message, Does.Contain("interno"));
    }

    [Test]
    public void ErrorController_Index_With401_ShouldReturnErrorView()
    {
        var loggerMock = new Mock<ILogger<ErrorController>>();
        var controller = new ErrorController(loggerMock.Object);

        var httpContext = new DefaultHttpContext();
        httpContext.Response.StatusCode = 401;
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var result = controller.Index(401);

        var viewResult = (ViewResult)result;
        var errorModel = (ErrorViewModel)viewResult.Model!;
        Assert.That(errorModel.Message, Does.Contain("Sesión expirada"));
    }
}
