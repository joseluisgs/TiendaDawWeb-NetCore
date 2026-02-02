using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Security.Claims;
using TiendaDawWeb.Controllers;
using TiendaDawWeb.Errors;
using TiendaDawWeb.Models;
using TiendaDawWeb.Models.Enums;
using TiendaDawWeb.Services.Carrito;
using TiendaDawWeb.Services.Product;
using TiendaDawWeb.Services.Purchase;
using FluentAssertions;

namespace TiendaDawWeb.Tests.Controllers;

/// <summary>
/// OBJETIVO: Validar el comportamiento del controlador del carrito de compras.
/// LO QUE BUSCA: Asegurar que las operaciones de añadir, eliminar y procesar compra funcionan.
/// </summary>
[TestFixture]
public class CarritoControllerTests
{
    private Mock<ICarritoService> _carritoServiceMock = null!;
    private Mock<IPurchaseService> _purchaseServiceMock = null!;
    private Mock<IProductService> _productServiceMock = null!;
    private Mock<UserManager<User>> _userManagerMock = null!;
    private Mock<ILogger<CarritoController>> _loggerMock = null!;
    private Mock<ITempDataDictionary> _tempDataMock = null!;
    private CarritoController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _carritoServiceMock = new Mock<ICarritoService>();
        _purchaseServiceMock = new Mock<IPurchaseService>();
        _productServiceMock = new Mock<IProductService>();

        var userStoreMock = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(
            userStoreMock.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);

        _loggerMock = new Mock<ILogger<CarritoController>>();
        _tempDataMock = new Mock<ITempDataDictionary>();

        _controller = new CarritoController(
            _carritoServiceMock.Object,
            _purchaseServiceMock.Object,
            _productServiceMock.Object,
            _userManagerMock.Object,
            _loggerMock.Object)
        {
            TempData = _tempDataMock.Object
        };

        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        }, "mock"));
        _controller.ControllerContext = new ControllerContext { HttpContext = context };
    }

    [TearDown]
    public void TearDown()
    {
        _controller?.Dispose();
    }

    /// <summary>
    /// PRUEBA: Index con usuario autenticado muestra el carrito.
    /// </summary>
    [Test]
    public async Task Index_ShouldReturnViewWithCarrito_WhenUserIsAuthenticated()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        var items = new List<CarritoItem>
        {
            new() { Id = 1, ProductoId = 1, Precio = 100, UsuarioId = 1 }
        };

        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        _carritoServiceMock.Setup(s => s.GetCarritoByUsuarioIdAsync(user.Id))
            .ReturnsAsync(Result.Success<IEnumerable<CarritoItem>, DomainError>(items));

        _carritoServiceMock.Setup(s => s.GetTotalCarritoAsync(user.Id))
            .ReturnsAsync(Result.Success<decimal, DomainError>(100));

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeAssignableTo<IEnumerable<CarritoItem>>();
        viewResult.ViewData["Total"].Should().Be(100m);
    }

    /// <summary>
    /// PRUEBA: Index sin usuario redirige a login.
    /// </summary>
    [Test]
    public async Task Index_ShouldRedirectToLogin_WhenUserNotFound()
    {
        // Arrange
        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _controller.Index();

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Login");
        redirectResult.ControllerName.Should().Be("Auth");
    }

    /// <summary>
    /// PRUEBA: Add producto válido al carrito.
    /// </summary>
    [Test]
    public async Task Add_ShouldAddProductToCarrito_WhenProductIsValid()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        var product = new Product { Id = 1, Nombre = "Test Product", Precio = 100, Deleted = false, Reservado = false };

        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        _productServiceMock.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(Result.Success<Product, DomainError>(product));

        _carritoServiceMock.Setup(s => s.AddToCarritoAsync(user.Id, 1))
            .ReturnsAsync(Result.Success<CarritoItem, DomainError>(new CarritoItem { Id = 1, ProductoId = 1 }));

        // Act
        var result = await _controller.Add(1);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Details");
        redirectResult.ControllerName.Should().Be("Product");
        _tempDataMock.VerifySet(t => t["Success"] = "Producto añadido al carrito");
    }

    /// <summary>
    /// PRUEBA: Add producto reservado muestra error.
    /// </summary>
    [Test]
    public async Task Add_ShouldShowError_WhenProductIsReserved()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        var product = new Product { Id = 1, Nombre = "Reserved Product", Precio = 100, Deleted = false, Reservado = true };

        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        _productServiceMock.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(Result.Success<Product, DomainError>(product));

        // Act
        var result = await _controller.Add(1);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        _tempDataMock.VerifySet(t => t["Error"] = "Este producto está reservado y no se puede añadir al carrito");
    }

    /// <summary>
    /// PRUEBA: Remove elimina item del carrito.
    /// </summary>
    [Test]
    public async Task Remove_ShouldRemoveItem_WhenSuccess()
    {
        // Arrange
        _carritoServiceMock.Setup(s => s.RemoveFromCarritoAsync(1))
            .ReturnsAsync(Result.Success<bool, DomainError>(true));

        // Act
        var result = await _controller.Remove(1);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Index");
        _tempDataMock.VerifySet(t => t["Success"] = "Producto eliminado del carrito");
    }

    /// <summary>
    /// PRUEBA: Remove con error muestra mensaje.
    /// </summary>
    [Test]
    public async Task Remove_ShouldShowError_WhenFails()
    {
        // Arrange
        _carritoServiceMock.Setup(s => s.RemoveFromCarritoAsync(1))
            .ReturnsAsync(Result.Failure<bool, DomainError>(CarritoError.ItemNotFound(1)));

        // Act
        var result = await _controller.Remove(1);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Index");
        _tempDataMock.VerifySet(t => t["Error"] = It.IsAny<string>());
    }

    /// <summary>
    /// PRUEBA: Clear vacía el carrito.
    /// </summary>
    [Test]
    public async Task Clear_ShouldClearCarrito_WhenSuccess()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        _carritoServiceMock.Setup(s => s.ClearCarritoAsync(user.Id))
            .ReturnsAsync(Result.Success<bool, DomainError>(true));

        // Act
        var result = await _controller.Clear();

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Index");
        _tempDataMock.VerifySet(t => t["Success"] = "Carrito vaciado");
    }

    /// <summary>
    /// PRUEBA: Finalizar compra exitosa.
    /// </summary>
    [Test]
    public async Task FinalizarCompra_ShouldRedirectToConfirmation_WhenSuccess()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        var purchase = new Purchase { Id = 100, CompradorId = 1, Total = 100 };

        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        _purchaseServiceMock.Setup(s => s.CreatePurchaseFromCarritoAsync(user.Id))
            .ReturnsAsync(Result.Success<Purchase, DomainError>(purchase));

        // Act
        var result = await _controller.FinalizarCompra();

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Confirmacion");
        redirectResult.ControllerName.Should().Be("Purchase");
        redirectResult.RouteValues!["id"].Should().Be(100);
        _tempDataMock.VerifySet(t => t["Success"] = "¡Compra realizada con éxito!");
    }

    /// <summary>
    /// PRUEBA: Finalizar compra fallida muestra error.
    /// </summary>
    [Test]
    public async Task FinalizarCompra_ShouldShowError_WhenFails()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        _purchaseServiceMock.Setup(s => s.CreatePurchaseFromCarritoAsync(user.Id))
            .ReturnsAsync(Result.Failure<Purchase, DomainError>(PurchaseError.EmptyCarrito()));

        // Act
        var result = await _controller.FinalizarCompra();

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Index");
        _tempDataMock.VerifySet(t => t["Error"] = "No se puede crear una compra con el carrito vacío");
    }

    /// <summary>
    /// PRUEBA: Resumen con carrito vacío redirige.
    /// </summary>
    [Test]
    public async Task Resumen_ShouldRedirectToIndex_WhenCarritoIsEmpty()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        _carritoServiceMock.Setup(s => s.GetCarritoByUsuarioIdAsync(user.Id))
            .ReturnsAsync(Result.Success<IEnumerable<CarritoItem>, DomainError>(new List<CarritoItem>()));

        // Act
        var result = await _controller.Resumen();

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Index");
        _tempDataMock.VerifySet(t => t["Error"] = "El carrito está vacío");
    }

    /// <summary>
    /// PRUEBA: AddToCart añade producto y redirige al índice.
    /// </summary>
    [Test]
    public async Task AddToCart_ShouldAddAndRedirectToIndex()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        _carritoServiceMock.Setup(s => s.AddToCarritoAsync(user.Id, 1))
            .ReturnsAsync(Result.Success<CarritoItem, DomainError>(new CarritoItem { Id = 1 }));

        // Act
        var result = await _controller.AddToCart(1);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Index");
        _tempDataMock.VerifySet(t => t["Success"] = "Producto añadido al carrito");
    }
}
