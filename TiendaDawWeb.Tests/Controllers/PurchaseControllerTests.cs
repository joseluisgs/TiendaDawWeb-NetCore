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
using TiendaDawWeb.Services.Purchase;
using FluentAssertions;

namespace TiendaDawWeb.Tests.Controllers;

/// <summary>
/// OBJETIVO: Validar el comportamiento del controlador de compras.
/// LO QUE BUSCA: Asegurar que la visualización de compras y descarga de PDFs funciona.
/// </summary>
[TestFixture]
public class PurchaseControllerTests
{
    private Mock<IPurchaseService> _purchaseServiceMock = null!;
    private Mock<UserManager<User>> _userManagerMock = null!;
    private Mock<ILogger<PurchaseController>> _loggerMock = null!;
    private Mock<ITempDataDictionary> _tempDataMock = null!;
    private PurchaseController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _purchaseServiceMock = new Mock<IPurchaseService>();

        var userStoreMock = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(
            userStoreMock.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);

        _loggerMock = new Mock<ILogger<PurchaseController>>();
        _tempDataMock = new Mock<ITempDataDictionary>();

        _controller = new PurchaseController(
            _purchaseServiceMock.Object,
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
    /// PRUEBA: Index con compras retorna vista con datos.
    /// </summary>
    [Test]
    public async Task Index_ShouldReturnViewWithPurchases_WhenUserHasPurchases()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        var purchases = new List<Purchase>
        {
            new() { Id = 1, CompradorId = 1, Total = 100 }
        };

        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        _purchaseServiceMock.Setup(s => s.GetByUserAsync(user.Id, 1))
            .ReturnsAsync(Result.Success<IEnumerable<Purchase>, DomainError>(purchases));

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeAssignableTo<IEnumerable<Purchase>>();
        viewResult.ViewData["CurrentPage"].Should().Be(1);
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
    /// PRUEBA: Details muestra la compra si el usuario es el comprador.
    /// </summary>
    [Test]
    public async Task Details_ShouldReturnView_WhenUserIsBuyer()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        var purchase = new Purchase { Id = 1, CompradorId = 1, Total = 100 };

        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        _purchaseServiceMock.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(Result.Success<Purchase, DomainError>(purchase));

        // Act
        var result = await _controller.Details(1);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<Purchase>();
    }

    /// <summary>
    /// PRUEBA: Details deniega acceso si el usuario no es el comprador.
    /// </summary>
    [Test]
    public async Task Details_ShouldRedirectWithError_WhenUserIsNotBuyer()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        var purchase = new Purchase { Id = 1, CompradorId = 2, Total = 100 }; // Different buyer

        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        _purchaseServiceMock.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(Result.Success<Purchase, DomainError>(purchase));

        // Act
        var result = await _controller.Details(1);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Index");
        _tempDataMock.VerifySet(t => t["Error"] = "No tienes permiso para ver esta compra");
    }

    /// <summary>
    /// PRUEBA: DownloadPdf retorna archivo PDF.
    /// </summary>
    [Test]
    public async Task DownloadPdf_ShouldReturnPdfFile_WhenUserIsAuthorized()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        var purchase = new Purchase { Id = 1, CompradorId = 1, Total = 100 };
        var pdfBytes = new byte[] { 1, 2, 3, 4, 5 };

        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        _purchaseServiceMock.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(Result.Success<Purchase, DomainError>(purchase));

        _purchaseServiceMock.Setup(s => s.GeneratePdfAsync(1))
            .ReturnsAsync(Result.Success<byte[], DomainError>(pdfBytes));

        // Act
        var result = await _controller.DownloadPdf(1);

        // Assert
        var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
        fileResult.ContentType.Should().Be("application/pdf");
        fileResult.FileDownloadName.Should().Be("factura-1.pdf");
    }

    /// <summary>
    /// PRUEBA: DownloadPdf deniega acceso si no es el comprador.
    /// </summary>
    [Test]
    public async Task DownloadPdf_ShouldRedirectWithError_WhenUserIsNotAuthorized()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        var purchase = new Purchase { Id = 1, CompradorId = 2, Total = 100 };

        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        _purchaseServiceMock.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(Result.Success<Purchase, DomainError>(purchase));

        // Act
        var result = await _controller.DownloadPdf(1);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        _tempDataMock.VerifySet(t => t["Error"] = "No tienes permiso para descargar esta factura");
    }

    /// <summary>
    /// PRUEBA: Confirmacion muestra página de confirmación.
    /// </summary>
    [Test]
    public async Task Confirmacion_ShouldReturnView_WhenUserIsBuyer()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        var purchase = new Purchase { Id = 1, CompradorId = 1, Total = 100 };

        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        _purchaseServiceMock.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(Result.Success<Purchase, DomainError>(purchase));

        // Act
        var result = await _controller.Confirmacion(1);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<Purchase>();
    }

    /// <summary>
    /// PRUEBA: Confirmacion deniega acceso si no es el comprador.
    /// </summary>
    [Test]
    public async Task Confirmacion_ShouldRedirectWithError_WhenUserIsNotBuyer()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        var purchase = new Purchase { Id = 1, CompradorId = 999, Total = 100 };

        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        _purchaseServiceMock.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(Result.Success<Purchase, DomainError>(purchase));

        // Act
        var result = await _controller.Confirmacion(1);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        _tempDataMock.VerifySet(t => t["Error"] = "No tienes permiso para ver esta compra");
    }

    /// <summary>
    /// PRUEBA: Details con compra no encontrada.
    /// </summary>
    [Test]
    public async Task Details_ShouldRedirectWithError_WhenPurchaseNotFound()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        _purchaseServiceMock.Setup(s => s.GetByIdAsync(99))
            .ReturnsAsync(Result.Failure<Purchase, DomainError>(PurchaseError.NotFound(99)));

        // Act
        var result = await _controller.Details(99);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        _tempDataMock.VerifySet(t => t["Error"] = It.IsAny<string>());
    }
}
