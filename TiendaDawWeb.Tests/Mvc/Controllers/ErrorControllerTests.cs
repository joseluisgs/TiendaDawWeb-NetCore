#nullable disable
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics;
using TiendaDawWeb.Controllers;
using TiendaDawWeb.Shared.ViewModels;

namespace TiendaDawWeb.Tests.Mvc.Controllers;

public class ErrorControllerTests
{
    private readonly Mock<ILogger<ErrorController>> _mockLogger;
    private readonly ErrorController _controller;

    public ErrorControllerTests()
    {
        _mockLogger = new Mock<ILogger<ErrorController>>();
        
        var httpContext = new DefaultHttpContext();
        var actionDescriptor = new ControllerActionDescriptor();
        var actionContext = new ActionContext(httpContext, new RouteData(), actionDescriptor);

        _controller = new ErrorController(_mockLogger.Object)
        {
            ControllerContext = new ControllerContext(actionContext)
        };
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _controller.Dispose();
    }

    [Test]
    public void Index_ReturnsErrorView_With404StatusCode()
    {
        var result = _controller.Index(404);

        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        var model = viewResult!.Model as ErrorViewModel;
        model!.StatusCode.Should().Be(404);
        model.Message.Should().Contain("no existe");
    }

    [Test]
    public void Index_ReturnsErrorView_With403StatusCode()
    {
        var result = _controller.Index(403);

        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        var model = viewResult!.Model as ErrorViewModel;
        model!.StatusCode.Should().Be(403);
        model.Message.Should().Contain("permisos");
    }

    [Test]
    public void Index_ReturnsErrorView_With500StatusCode()
    {
        var result = _controller.Index(500);

        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        var model = viewResult!.Model as ErrorViewModel;
        model!.StatusCode.Should().Be(500);
        model.Message.Should().Contain("interno");
    }

    [Test]
    public void Index_SetsRequestId()
    {
        var result = _controller.Index(404);

        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        var model = viewResult!.Model as ErrorViewModel;
        model!.RequestId.Should().NotBeNullOrEmpty();
    }
}
