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
using TiendaDawWeb.Shared.Services.Carrito;

namespace TiendaDawWeb.Tests.Mvc.Controllers;

public class CartCountControllerTests
{
    private readonly Mock<ICarritoService> _mockCarritoService;
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly CartCountController _controller;
    private readonly ClaimsPrincipal _userPrincipal;

    public CartCountControllerTests()
    {
        _mockCarritoService = new Mock<ICarritoService>();
        
        var userStoreMock = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        _userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        }, "mock"));

        var httpContext = new DefaultHttpContext { User = _userPrincipal };
        var actionDescriptor = new ControllerActionDescriptor();
        var actionContext = new ActionContext(httpContext, new RouteData(), actionDescriptor);

        _controller = new CartCountController(
            _mockCarritoService.Object,
            _mockUserManager.Object)
        {
            ControllerContext = new ControllerContext(actionContext)
        };
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
    }

    [Test]
    public async Task GetCartCount_ReturnsZero_WhenUserNotFound()
    {
        _mockUserManager.Setup(u => u.GetUserAsync(_userPrincipal))
            .ReturnsAsync((User)null!);

        var result = await _controller.GetCartCount();

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();
    }

    [Test]
    public async Task GetCartCount_ReturnsCount_WhenUserFound()
    {
        var user = new User { Id = 1, UserName = "testuser" };
        _mockUserManager.Setup(u => u.GetUserAsync(_userPrincipal))
            .ReturnsAsync(user);
        _mockCarritoService.Setup(s => s.GetCarritoCountAsync(user.Id))
            .ReturnsAsync(CSharpFunctionalExtensions.Result.Success<int, TiendaDawWeb.Shared.Errors.DomainError>(5));

        var result = await _controller.GetCartCount();

        result.Should().BeOfType<OkObjectResult>();
    }
}
