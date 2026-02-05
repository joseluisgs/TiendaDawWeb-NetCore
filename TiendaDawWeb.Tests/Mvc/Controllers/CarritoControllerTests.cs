#nullable disable
using System.Security.Claims;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using TiendaDawWeb.Controllers;
using TiendaDawWeb.Shared.Errors;
using TiendaDawWeb.Shared.Models;
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
    private readonly ClaimsPrincipal _userPrincipal;

    public CarritoControllerTests()
    {
        _mockCarritoService = new Mock<ICarritoService>();
        _mockPurchaseService = new Mock<IPurchaseService>();
        _mockProductService = new Mock<IProductService>();
        
        var userStoreMock = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        _mockLogger = new Mock<ILogger<CarritoController>>();
        
        _userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        }, "mock"));

        var httpContext = new DefaultHttpContext { User = _userPrincipal };
        var actionContext = new ActionContext(httpContext, new RouteData(), new ControllerActionDescriptor());

        _controller = new CarritoController(
            _mockCarritoService.Object,
            _mockPurchaseService.Object,
            _mockProductService.Object,
            _mockUserManager.Object,
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

    #region Constructor Tests

    [Test]
    public void Constructor_CreatesInstance()
    {
        _controller.Should().NotBeNull();
    }

    [Test]
    public void CarritoController_InheritsFromController()
    {
        typeof(CarritoController).Should().BeDerivedFrom<Controller>();
    }

    #endregion

    #region Index Tests

    [Test]
    public async Task Index_RedirectsToLogin_WhenUserNotFound()
    {
        _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((User)null!);

        var result = await _controller.Index();

        result.Should().BeOfType<RedirectToActionResult>();
        (result as RedirectToActionResult)!.ActionName.Should().Be("Login");
    }

    #endregion

    #region Add Tests

    [Test]
    public async Task Add_RedirectsToLogin_WhenUserNotFound()
    {
        _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((User)null!);

        var result = await _controller.Add(1);

        result.Should().BeOfType<RedirectToActionResult>();
    }

    #endregion

    #region AddToCart Tests

    [Test]
    public async Task AddToCart_RedirectsToLogin_WhenUserNotFound()
    {
        _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((User)null!);

        var result = await _controller.AddToCart(1);

        result.Should().BeOfType<RedirectToActionResult>();
    }

    #endregion

    #region Remove Tests

    [Test]
    public void Remove_HasCorrectSignature()
    {
        var method = typeof(CarritoController).GetMethod("Remove");
        method.Should().NotBeNull();
    }

    #endregion

    #region Clear Tests

    [Test]
    public async Task Clear_RedirectsToLogin_WhenUserNotFound()
    {
        _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((User)null!);

        var result = await _controller.Clear();

        result.Should().BeOfType<RedirectToActionResult>();
    }

    #endregion

    #region Resumen Tests

    [Test]
    public async Task Resumen_RedirectsToLogin_WhenUserNotFound()
    {
        _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((User)null!);

        var result = await _controller.Resumen();

        result.Should().BeOfType<RedirectToActionResult>();
    }

    #endregion

    #region FinalizarCompra Tests

    [Test]
    public async Task FinalizarCompra_RedirectsToLogin_WhenUserNotFound()
    {
        _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((User)null!);

        var result = await _controller.FinalizarCompra();

        result.Should().BeOfType<RedirectToActionResult>();
    }

    #endregion
}
