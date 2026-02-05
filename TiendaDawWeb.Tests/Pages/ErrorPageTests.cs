using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NUnit.Framework;
using TiendaDawWeb.RazorPages.Pages;

namespace TiendaDawWeb.Tests.RazorPages.ErrorHandling;

public class ErrorPageTests
{
    [Test]
    public void ErrorModel_OnGet_With404_ShouldShowNotFoundMessage()
    {
        // Arrange
        var pageModel = new ErrorModel();

        var httpContext = new DefaultHttpContext();
        httpContext.Response.StatusCode = 404;
        pageModel.PageContext = new PageContext
        {
            HttpContext = httpContext
        };

        // Act
        pageModel.OnGet(404);

        // Assert
        Assert.That(pageModel.StatusCodeValue, Is.EqualTo(404));
        Assert.That(pageModel.Message, Does.Contain("no existe"));
        Assert.That(pageModel.ShowRequestId, Is.True);
    }

    [Test]
    public void ErrorModel_OnGet_With500_ShouldShowInternalErrorMessage()
    {
        // Arrange
        var pageModel = new ErrorModel();

        var httpContext = new DefaultHttpContext();
        httpContext.Response.StatusCode = 500;
        pageModel.PageContext = new PageContext
        {
            HttpContext = httpContext
        };

        // Act
        pageModel.OnGet(500);

        // Assert
        Assert.That(pageModel.StatusCodeValue, Is.EqualTo(500));
        Assert.That(pageModel.Message, Does.Contain("interno"));
    }

    [Test]
    public void ErrorModel_OnGet_With401_ShouldShowSessionExpiredMessage()
    {
        // Arrange
        var pageModel = new ErrorModel();

        var httpContext = new DefaultHttpContext();
        httpContext.Response.StatusCode = 401;
        pageModel.PageContext = new PageContext
        {
            HttpContext = httpContext
        };

        // Act
        pageModel.OnGet(401);

        // Assert
        Assert.That(pageModel.StatusCodeValue, Is.EqualTo(401));
        Assert.That(pageModel.Message, Does.Contain("Sesión expirada"));
    }

    [Test]
    public void ErrorModel_OnGet_WithNullStatusCode_ShouldUseHttpContextStatusCode()
    {
        // Arrange
        var pageModel = new ErrorModel();

        var httpContext = new DefaultHttpContext();
        httpContext.Response.StatusCode = 403;
        pageModel.PageContext = new PageContext
        {
            HttpContext = httpContext
        };

        // Act
        pageModel.OnGet(null);

        // Assert
        Assert.That(pageModel.StatusCodeValue, Is.EqualTo(403));
        Assert.That(pageModel.Message, Does.Contain("permisos"));
    }

    [Test]
    public void ErrorModel_OnGet_With200_ShouldConvertTo500()
    {
        // Arrange
        var pageModel = new ErrorModel();

        var httpContext = new DefaultHttpContext();
        httpContext.Response.StatusCode = 200;
        pageModel.PageContext = new PageContext
        {
            HttpContext = httpContext
        };

        // Act
        pageModel.OnGet(null);

        // Assert
        Assert.That(pageModel.StatusCodeValue, Is.EqualTo(500));
    }
}
