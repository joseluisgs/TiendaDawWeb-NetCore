using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TiendaDawWeb.Controllers;
using TiendaDawWeb.Shared.Errors;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Purchase;

namespace TiendaDawWeb.Tests.Mvc.Controllers;

public class PurchaseControllerTests
{
    private readonly Mock<IPurchaseService> _mockPurchaseService;
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly Mock<ILogger<PurchaseController>> _mockLogger;
    private readonly PurchaseController _controller;

    public PurchaseControllerTests()
    {
        _mockPurchaseService = new Mock<IPurchaseService>();
        
        var userStoreMock = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        _mockLogger = new Mock<ILogger<PurchaseController>>();
        
        _controller = new PurchaseController(
            _mockPurchaseService.Object,
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

    [Test]
    public async Task Index_ReturnsView_WhenPurchasesExist()
    {
        var user = new User { Id = 1, Email = "test@test.com" };
        var purchases = new List<Purchase>
        {
            new Purchase { Id = 1, CompradorId = 1, Total = 100 }
        };
        
        _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync(user);
        _mockPurchaseService.Setup(s => s.GetByUserAsync(1, 1))
            .Returns(Task.FromResult(Result.Success<IEnumerable<Purchase>, DomainError>(purchases)));

        var result = await _controller.Index();

        result.Should().BeOfType<ViewResult>();
    }

    #endregion

    #region Details Tests

    [Test]
    public async Task Details_RedirectsToLogin_WhenUserNotFound()
    {
        _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync((User)null!);

        var result = await _controller.Details(1);

        result.Should().BeOfType<RedirectToActionResult>();
    }

    [Test]
    public async Task Details_ReturnsView_WhenPurchaseExists()
    {
        var user = new User { Id = 1, Email = "test@test.com" };
        var purchase = new Purchase { Id = 1, CompradorId = 1, Total = 100 };
        
        _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync(user);
        _mockPurchaseService.Setup(s => s.GetByIdAsync(1))
            .Returns(Task.FromResult(Result.Success<Purchase, DomainError>(purchase)));

        var result = await _controller.Details(1);

        result.Should().BeOfType<ViewResult>();
    }

    #endregion
}
