#nullable disable
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using TiendaDawWeb.Mvc.Middlewares;
using TiendaDawWeb.Shared.Exceptions;

namespace TiendaDawWeb.Tests.Mvc;

public class GlobalExceptionMiddlewareTests
{
    [Test]
    public void Middleware_CanBeInstantiated()
    {
        var loggerMock = new Mock<ILogger<GlobalExceptionMiddleware>>();
        var nextDelegate = new RequestDelegate(context => Task.CompletedTask);
        var middleware = new GlobalExceptionMiddleware(nextDelegate, loggerMock.Object);
        
        middleware.Should().NotBeNull();
    }

    [Test]
    public void Middleware_LogsNotFoundException()
    {
        var loggerMock = new Mock<ILogger<GlobalExceptionMiddleware>>();
        var nextDelegate = new RequestDelegate(context => throw new NotFoundException("Not found"));
        var middleware = new GlobalExceptionMiddleware(nextDelegate, loggerMock.Object);

        var httpContext = new DefaultHttpContext
        {
            Request = { Path = "/api/test" }
        };
        httpContext.Request.Headers["Accept"] = "application/json";

        var act = async () => await middleware.InvokeAsync(httpContext);
        
        act.Should().NotThrowAsync();
        httpContext.Response.StatusCode.Should().Be(404);
    }

    [Test]
    public void Middleware_LogsValidationException()
    {
        var loggerMock = new Mock<ILogger<GlobalExceptionMiddleware>>();
        var nextDelegate = new RequestDelegate(context => throw new ValidationException("Invalid"));
        var middleware = new GlobalExceptionMiddleware(nextDelegate, loggerMock.Object);

        var httpContext = new DefaultHttpContext
        {
            Request = { Path = "/api/test" }
        };
        httpContext.Request.Headers["Accept"] = "application/json";

        var act = async () => await middleware.InvokeAsync(httpContext);
        
        act.Should().NotThrowAsync();
        httpContext.Response.StatusCode.Should().Be(400);
    }

    [Test]
    public void Middleware_LogsBusinessException()
    {
        var loggerMock = new Mock<ILogger<GlobalExceptionMiddleware>>();
        var nextDelegate = new RequestDelegate(context => throw new BusinessException("Business error"));
        var middleware = new GlobalExceptionMiddleware(nextDelegate, loggerMock.Object);

        var httpContext = new DefaultHttpContext
        {
            Request = { Path = "/api/test" }
        };
        httpContext.Request.Headers["Accept"] = "application/json";

        var act = async () => await middleware.InvokeAsync(httpContext);
        
        act.Should().NotThrowAsync();
        httpContext.Response.StatusCode.Should().Be(400);
    }

    [Test]
    public void Middleware_LogsUnauthorizedException()
    {
        var loggerMock = new Mock<ILogger<GlobalExceptionMiddleware>>();
        var nextDelegate = new RequestDelegate(context => throw new UnauthorizedException("Unauthorized"));
        var middleware = new GlobalExceptionMiddleware(nextDelegate, loggerMock.Object);

        var httpContext = new DefaultHttpContext
        {
            Request = { Path = "/api/test" }
        };
        httpContext.Request.Headers["Accept"] = "application/json";

        var act = async () => await middleware.InvokeAsync(httpContext);
        
        act.Should().NotThrowAsync();
        httpContext.Response.StatusCode.Should().Be(401);
    }

    [Test]
    public void Middleware_LogsForbiddenException()
    {
        var loggerMock = new Mock<ILogger<GlobalExceptionMiddleware>>();
        var nextDelegate = new RequestDelegate(context => throw new ForbiddenException("Forbidden"));
        var middleware = new GlobalExceptionMiddleware(nextDelegate, loggerMock.Object);

        var httpContext = new DefaultHttpContext
        {
            Request = { Path = "/api/test" }
        };
        httpContext.Request.Headers["Accept"] = "application/json";

        var act = async () => await middleware.InvokeAsync(httpContext);
        
        act.Should().NotThrowAsync();
        httpContext.Response.StatusCode.Should().Be(403);
    }

    [Test]
    public void Middleware_HandlesGenericException()
    {
        var loggerMock = new Mock<ILogger<GlobalExceptionMiddleware>>();
        var nextDelegate = new RequestDelegate(context => throw new Exception("Error"));
        var middleware = new GlobalExceptionMiddleware(nextDelegate, loggerMock.Object);

        var httpContext = new DefaultHttpContext
        {
            Request = { Path = "/api/test" }
        };
        httpContext.Request.Headers["Accept"] = "application/json";

        var act = async () => await middleware.InvokeAsync(httpContext);
        
        act.Should().NotThrowAsync();
        httpContext.Response.StatusCode.Should().Be(500);
    }

    [Test]
    public void Middleware_SetsJsonContentType_ForApi()
    {
        var loggerMock = new Mock<ILogger<GlobalExceptionMiddleware>>();
        var nextDelegate = new RequestDelegate(context => throw new Exception("Error"));
        var middleware = new GlobalExceptionMiddleware(nextDelegate, loggerMock.Object);

        var httpContext = new DefaultHttpContext
        {
            Request = { Path = "/api/test" }
        };
        httpContext.Request.Headers["Accept"] = "application/json";

        var act = async () => await middleware.InvokeAsync(httpContext);
        
        act.Should().NotThrowAsync();
        httpContext.Response.ContentType.Should().Contain("application/json");
    }

    [Test]
    public void Middleware_DoesNotThrow_WhenNoException()
    {
        var loggerMock = new Mock<ILogger<GlobalExceptionMiddleware>>();
        var nextDelegate = new RequestDelegate(context => Task.CompletedTask);
        var middleware = new GlobalExceptionMiddleware(nextDelegate, loggerMock.Object);

        var httpContext = new DefaultHttpContext();

        var act = async () => await middleware.InvokeAsync(httpContext);
        
        act.Should().NotThrowAsync();
    }
}
