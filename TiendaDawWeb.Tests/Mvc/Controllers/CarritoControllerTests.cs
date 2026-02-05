using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TiendaDawWeb.Controllers;
using TiendaDawWeb.Shared.Errors;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Models.Enums;
using TiendaDawWeb.Shared.Services.Carrito;
using TiendaDawWeb.Shared.Services.Product;
using TiendaDawWeb.Shared.Services.Purchase;

namespace TiendaDawWeb.Tests.Mvc.Controllers;

public class CarritoControllerTests
{
    private readonly Mock<ICarritoService> _mockCarritoService;
    private readonly Mock<IPurchaseService> _mockPurchaseService;
    private readonly Mock<IProductService> _mockProductService;
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly Mock<ILogger<CarritoController>> _mockLogger;
    private readonly CarritoController _controller;

    public CarritoControllerTests()
    {
        _mockCarritoService = new Mock<ICarritoService>();
        _mockPurchaseService = new Mock<IPurchaseService>();
        _mockProductService = new Mock<IProductService>();
        
        var userStoreMock = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        _mockLogger = new Mock<ILogger<CarritoController>>();
        
        _controller = new CarritoController(
            _mockCarritoService.Object,
            _mockPurchaseService.Object,
            _mockProductService.Object,
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
        (result as RedirectToActionResult)!.ActionName.Should().Be("Login");
    }

    [Test]
    public async Task Index_ReturnsViewWithCarritoItems()
    {
        var user = new User { Id = 1, Email = "test@test.com" };
        var items = new List<CarritoItem>
        {
            new CarritoItem { Id = 1, UsuarioId = 1, ProductoId = 1, Precio = 100 }
        };
        
        _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync(user);
        _mockCarritoService.Setup(s => s.GetCarritoByUsuarioIdAsync(1))
            .Returns(Task.FromResult(Result.Success<IEnumerable<CarritoItem>, DomainError>(items)));
        _mockCarritoService.Setup(s => s.GetTotalCarritoAsync(1))
            .Returns(Task.FromResult(Result.Success<decimal, DomainError>(100)));

        var result = await _controller.Index();

        result.Should().BeOfType<ViewResult>();
    }

    #endregion

    #region Add Tests

    [Test]
    public async Task Add_RedirectsToLogin_WhenUserNotFound()
    {
        _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync((User)null!);

        var result = await _controller.Add(1);

        result.Should().BeOfType<RedirectToActionResult>();
    }

    #endregion

    #region Remove Tests

    #endregion

    #region Clear Tests

    [Test]
    public async Task Clear_RedirectsToLogin_WhenUserNotFound()
    {
        _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync((User)null!);

        var result = await _controller.Clear();

        result.Should().BeOfType<RedirectToActionResult>();
    }

    #endregion

    #region FinalizarCompra Tests

    [Test]
    public async Task FinalizarCompra_RedirectsToLogin_WhenUserNotFound()
    {
        _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync((User)null!);

        var result = await _controller.FinalizarCompra();

        result.Should().BeOfType<RedirectToActionResult>();
    }

    #endregion
}
