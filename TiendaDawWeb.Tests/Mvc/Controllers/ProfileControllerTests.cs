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
using TiendaDawWeb.Shared.Data;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Storage;

namespace TiendaDawWeb.Tests.Mvc.Controllers;

public class ProfileControllerTests
{
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly Mock<ApplicationDbContext> _mockContext;
    private readonly Mock<IStorageService> _mockStorageService;
    private readonly Mock<ILogger<ProfileController>> _mockLogger;
    private readonly ProfileController _controller;
    private readonly ClaimsPrincipal _userPrincipal;

    public ProfileControllerTests()
    {
        var userStoreMock = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        _mockContext = new Mock<ApplicationDbContext>(new Microsoft.EntityFrameworkCore.DbContextOptions<ApplicationDbContext>());
        _mockStorageService = new Mock<IStorageService>();
        _mockLogger = new Mock<ILogger<ProfileController>>();
        
        _userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        }, "mock"));

        var httpContext = new DefaultHttpContext { User = _userPrincipal };
        var actionDescriptor = new ControllerActionDescriptor();
        var actionContext = new ActionContext(httpContext, new RouteData(), actionDescriptor);

        _controller = new ProfileController(
            _mockUserManager.Object,
            _mockContext.Object,
            _mockStorageService.Object,
            _mockLogger.Object)
        {
            ControllerContext = new ControllerContext(actionContext)
        };
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _controller.Dispose();
    }

    #region Index Tests

    [Test]
    public async Task Index_RedirectsToLogin_WhenUserNotFound()
    {
        _mockUserManager.Setup(u => u.GetUserAsync(_userPrincipal))
            .ReturnsAsync((User)null!);

        var result = await _controller.Index();

        result.Should().BeOfType<RedirectToActionResult>();
        (result as RedirectToActionResult)!.ActionName.Should().Be("Login");
    }

    #endregion

    #region Edit (GET) Tests

    [Test]
    public async Task Edit_RedirectsToLogin_WhenUserNotFound()
    {
        _mockUserManager.Setup(u => u.GetUserAsync(_userPrincipal))
            .ReturnsAsync((User)null!);

        var result = await _controller.Edit();

        result.Should().BeOfType<RedirectToActionResult>();
        (result as RedirectToActionResult)!.ActionName.Should().Be("Login");
    }

    [Test]
    public async Task Edit_ReturnsView_WhenUserFound()
    {
        var user = new User { Id = 1, UserName = "testuser" };
        _mockUserManager.Setup(u => u.GetUserAsync(_userPrincipal))
            .ReturnsAsync(user);

        var result = await _controller.Edit();

        result.Should().BeOfType<ViewResult>();
        (result as ViewResult)!.Model.Should().Be(user);
    }

    #endregion

    #region ChangePassword (GET) Tests

    [Test]
    public void ChangePassword_ReturnsView()
    {
        var result = _controller.ChangePassword();

        result.Should().BeOfType<ViewResult>();
    }

    #endregion
}
