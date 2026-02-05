using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using TiendaDawWeb.Controllers;
using TiendaDawWeb.Shared.Errors;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Models.Enums;
using TiendaDawWeb.Shared.Services.Favorite;
using TiendaDawWeb.Shared.Services.Product;
using TiendaDawWeb.Shared.Services.Storage;
using TiendaDawWeb.Shared.ViewModels;
using TiendaDawWeb.Shared.Web.Hubs;

namespace TiendaDawWeb.Tests.Mvc.Controllers;

public class ProductControllerTests
{
    private readonly Mock<IProductService> _mockProductService;
    private readonly Mock<IStorageService> _mockStorageService;
    private readonly Mock<IFavoriteService> _mockFavoriteService;
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly Mock<IHubContext<NotificationHub>> _mockHubContext;
    private readonly Mock<ILogger<ProductController>> _mockLogger;
    private readonly ProductController _controller;

    public ProductControllerTests()
    {
        _mockProductService = new Mock<IProductService>();
        _mockStorageService = new Mock<IStorageService>();
        _mockFavoriteService = new Mock<IFavoriteService>();
        
        var userStoreMock = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        _mockHubContext = new Mock<IHubContext<NotificationHub>>();
        _mockLogger = new Mock<ILogger<ProductController>>();
        
        _controller = new ProductController(
            _mockProductService.Object,
            _mockStorageService.Object,
            _mockFavoriteService.Object,
            _mockUserManager.Object,
            _mockHubContext.Object,
            _mockLogger.Object);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _controller.Dispose();
    }

    #region Create Tests

    [Test]
    public void Create_ReturnsView()
    {
        var result = _controller.Create();
        result.Should().BeOfType<ViewResult>();
    }

    #endregion

    #region MyProducts Tests

    [Test]
    public async Task MyProducts_RedirectsToLogin_WhenUserNotFound()
    {
        _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync((User)null!);

        var result = await _controller.MyProducts();

        result.Should().BeOfType<RedirectToActionResult>();
    }

    #endregion

    #region Delete Tests

    [Test]
    public async Task Delete_RedirectsToLogin_WhenUserNotFound()
    {
        _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync((User)null!);

        var result = await _controller.Delete(1);

        result.Should().BeOfType<RedirectToActionResult>();
    }

    #endregion
}
