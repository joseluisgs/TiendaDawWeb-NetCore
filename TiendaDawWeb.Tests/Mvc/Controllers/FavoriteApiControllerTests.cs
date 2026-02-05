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
using TiendaDawWeb.Shared.Services.Favorite;

namespace TiendaDawWeb.Tests.Mvc.Controllers;

public class FavoriteApiControllerTests
{
    private readonly Mock<IFavoriteService> _mockFavoriteService;
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly Mock<ILogger<FavoriteApiController>> _mockLogger;
    private readonly FavoriteApiController _controller;
    private readonly ClaimsPrincipal _userPrincipal;

    public FavoriteApiControllerTests()
    {
        _mockFavoriteService = new Mock<IFavoriteService>();
        
        var userStoreMock = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        _mockLogger = new Mock<ILogger<FavoriteApiController>>();
        
        _userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        }, "mock"));

        var httpContext = new DefaultHttpContext { User = _userPrincipal };
        var actionDescriptor = new ControllerActionDescriptor();
        var actionContext = new ActionContext(httpContext, new RouteData(), actionDescriptor);

        _controller = new FavoriteApiController(
            _mockFavoriteService.Object,
            _mockUserManager.Object,
            _mockLogger.Object)
        {
            ControllerContext = new ControllerContext(actionContext)
        };
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
    }

    [Test]
    public async Task CheckFavorite_ReturnsUnauthorized_WhenUserNotFound()
    {
        _mockUserManager.Setup(u => u.GetUserAsync(_userPrincipal))
            .ReturnsAsync((User)null!);

        var result = await _controller.CheckFavorite(1);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Test]
    public async Task CheckFavorite_ReturnsIsFavorite_WhenSuccessful()
    {
        var user = new User { Id = 1, UserName = "testuser" };
        _mockUserManager.Setup(u => u.GetUserAsync(_userPrincipal))
            .ReturnsAsync(user);
        _mockFavoriteService.Setup(s => s.IsFavoriteAsync(user.Id, 1))
            .ReturnsAsync(CSharpFunctionalExtensions.Result.Success<bool, TiendaDawWeb.Shared.Errors.DomainError>(true));

        var result = await _controller.CheckFavorite(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task RemoveFavorite_ReturnsUnauthorized_WhenUserNotFound()
    {
        _mockUserManager.Setup(u => u.GetUserAsync(_userPrincipal))
            .ReturnsAsync((User)null!);

        var result = await _controller.RemoveFavorite(1);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Test]
    public async Task AddFavorite_ReturnsUnauthorized_WhenUserNotFound()
    {
        _mockUserManager.Setup(u => u.GetUserAsync(_userPrincipal))
            .ReturnsAsync((User)null!);

        var result = await _controller.AddFavorite(new AddFavoriteRequest(1));

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }
}
