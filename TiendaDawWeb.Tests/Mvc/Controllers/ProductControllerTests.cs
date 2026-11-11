using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Logging;
using Moq;
using TiendaDawWeb.Controllers;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Favorite;
using TiendaDawWeb.Shared.Services.Product;
using TiendaDawWeb.Shared.Services.Storage;

namespace TiendaDawWeb.Tests.Mvc.Controllers;

public class ProductControllerTests {
    private readonly ProductController _controller;
    private readonly Mock<IFavoriteService> _mockFavoriteService;
    private readonly Mock<ILogger<ProductController>> _mockLogger;
    private readonly Mock<IProductService> _mockProductService;
    private readonly Mock<IStorageService> _mockStorageService;
    private readonly Mock<UserManager<User>> _mockUserManager;

    public ProductControllerTests() {
        _mockProductService = new Mock<IProductService>();
        _mockStorageService = new Mock<IStorageService>();
        _mockFavoriteService = new Mock<IFavoriteService>();

        var userStoreMock = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!,
            null!, null!);

        _mockLogger = new Mock<ILogger<ProductController>>();

        var outputCacheStoreMock = new Mock<IOutputCacheStore>();

        _controller = new ProductController(
            _mockProductService.Object,
            _mockStorageService.Object,
            _mockFavoriteService.Object,
            _mockUserManager.Object,
            outputCacheStoreMock.Object,
            _mockLogger.Object);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown() {
        _controller.Dispose();
    }

    #region Create Tests

    [Test]
    public void Create_ReturnsView() {
        IActionResult result = _controller.Create();
        result.Should().BeOfType<ViewResult>();
    }

    #endregion

    #region MyProducts Tests

    [Test]
    public async Task MyProducts_RedirectsToLogin_WhenUserNotFound() {
        _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((User)null!);

        IActionResult result = await _controller.MyProducts();

        result.Should().BeOfType<RedirectToActionResult>();
    }

    #endregion

    #region Delete Tests

    [Test]
    public async Task Delete_RedirectsToLogin_WhenUserNotFound() {
        _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((User)null!);

        IActionResult result = await _controller.Delete(1);

        result.Should().BeOfType<RedirectToActionResult>();
    }

    #endregion
}
