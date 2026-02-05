using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TiendaDawWeb.Controllers;
using TiendaDawWeb.Shared.Errors;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Favorite;

namespace TiendaDawWeb.Tests.Mvc.Controllers;

public class FavoriteControllerTests
{
    private readonly Mock<IFavoriteService> _mockFavoriteService;
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly Mock<ILogger<FavoriteController>> _mockLogger;
    private readonly FavoriteController _controller;

    public FavoriteControllerTests()
    {
        _mockFavoriteService = new Mock<IFavoriteService>();
        
        var userStoreMock = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        _mockLogger = new Mock<ILogger<FavoriteController>>();
        
        _controller = new FavoriteController(
            _mockFavoriteService.Object,
            _mockUserManager.Object,
            _mockLogger.Object);
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
        _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync((User)null!);

        var result = await _controller.Index();

        result.Should().BeOfType<RedirectToActionResult>();
    }

    #endregion

    #region Add Tests

    [Test]
    public async Task Add_RedirectsToLogin_WhenUserNotFound()
    {
        _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync((User)null!);

        var result = await _controller.Add(1);

        result.Should().BeOfType<JsonResult>();
    }

    #endregion

    #region Remove Tests

    [Test]
    public async Task Remove_ReturnsJsonResult()
    {
        _mockFavoriteService.Setup(s => s.RemoveFavoriteAsync(1, 1))
            .Returns(Task.FromResult(Result.Success<bool, DomainError>(true)));

        var result = await _controller.Remove(1);

        result.Should().BeOfType<JsonResult>();
    }

    #endregion
}
