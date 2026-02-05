#nullable disable
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using TiendaDawWeb.Controllers;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.ViewModels;

namespace TiendaDawWeb.Tests.Mvc.Controllers;

public class AuthControllerTests
{
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly Mock<SignInManager<User>> _mockSignInManager;
    private readonly Mock<ILogger<AuthController>> _mockLogger;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        var userStoreMock = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        var contextAccessor = new Mock<Microsoft.AspNetCore.Identity.IUserClaimsPrincipalFactory<User>>();
        _mockSignInManager = new Mock<SignInManager<User>>(
            _mockUserManager.Object,
            new HttpContextAccessor(),
            contextAccessor.Object,
            null!, null!, null!, null!);
        
        _mockLogger = new Mock<ILogger<AuthController>>();
        
        _controller = new AuthController(
            _mockUserManager.Object,
            _mockSignInManager.Object,
            _mockLogger.Object);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _controller.Dispose();
    }

    #region Login Tests

    [Test]
    public void Login_GET_ReturnsView_WithReturnUrl()
    {
        var httpContext = new DefaultHttpContext();
        var controller = new AuthController(
            _mockUserManager.Object,
            _mockSignInManager.Object,
            _mockLogger.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };

        var result = controller.Login("/return/url");

        result.Should().BeOfType<ViewResult>();
        controller.ViewData["ReturnUrl"].Should().Be("/return/url");
    }

    [Test]
    public void Login_ReturnsView_WithNullReturnUrl()
    {
        var result = _controller.Login(default(string));

        result.Should().BeOfType<ViewResult>();
    }

    #endregion

    #region Register Tests

    [Test]
    public void Register_ReturnsView()
    {
        var result = _controller.Register();

        result.Should().BeOfType<ViewResult>();
    }

    #endregion

    #region AccessDenied Tests

    [Test]
    public void AccessDenied_ReturnsView()
    {
        var result = _controller.AccessDenied();

        result.Should().BeOfType<ViewResult>();
    }

    #endregion
}
