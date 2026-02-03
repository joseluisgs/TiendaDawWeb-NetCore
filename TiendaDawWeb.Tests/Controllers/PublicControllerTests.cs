using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Security.Claims;
using TiendaDawWeb.Controllers;
using TiendaDawWeb.Models;
using TiendaDawWeb.Services.Product;
using FluentAssertions;
using CSharpFunctionalExtensions;

namespace TiendaDawWeb.Tests.Controllers;

/// <summary>
/// OBJETIVO: Validar el controlador de páginas públicas.
/// LO QUE BUSCA: Asegurar que la página principal y sus filtros funcionan correctamente.
/// </summary>
[TestFixture]
public class PublicControllerTests
{
    private Mock<IProductService> _productServiceMock = null!;
    private Mock<ILogger<PublicController>> _loggerMock = null!;
    private PublicController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _productServiceMock = new Mock<IProductService>();
        _loggerMock = new Mock<ILogger<PublicController>>();
        
        // Crear controller manualmente para evitar problemas con primary constructors
        _controller = new PublicController(_productServiceMock.Object, _loggerMock.Object);
        
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    [TearDown]
    public void TearDown()
    {
        _controller?.Dispose();
    }

    /// <summary>
    /// PRUEBA: Index muestra productos disponibles (caso básico).
    /// </summary>
    [Test]
    public async Task Index_ShouldReturnViewWithProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Nombre = "Product 1", Precio = 100 },
            new() { Id = 2, Nombre = "Product 2", Precio = 200 }
        };
        _productServiceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(Result.Success<IEnumerable<Product>, TiendaDawWeb.Errors.DomainError>(products));

        // Act - Llamar con todos los parámetros
        var result = await InvokeIndex(null, null, null, null, 1, 12, null);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeAssignableTo<IEnumerable<Product>>();
    }

    /// <summary>
    /// PRUEBA: Index retorna lista vacía si falla el servicio.
    /// </summary>
    [Test]
    public async Task Index_ShouldReturnEmptyList_WhenServiceFails()
    {
        // Arrange
        _productServiceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(Result.Failure<IEnumerable<Product>, TiendaDawWeb.Errors.DomainError>(
                new TiendaDawWeb.Errors.NotFoundError("Error")));

        // Act
        var result = await InvokeIndex(null, null, null, null, 1, 12, null);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeAssignableTo<IEnumerable<Product>>();
    }

    /// <summary>
    /// PRUEBA: Index redirige cuando lang es "en".
    /// </summary>
    [Test]
    public async Task Index_ShouldRedirect_WhenLangIsEnglish()
    {
        // Arrange
        var products = new List<Product>();
        _productServiceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(Result.Success<IEnumerable<Product>, TiendaDawWeb.Errors.DomainError>(products));

        // Act
        var result = await InvokeIndex(null, null, null, null, 1, 12, "en");

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Index");
    }

    /// <summary>
    /// PRUEBA: Index redirige cuando lang es "es".
    /// </summary>
    [Test]
    public async Task Index_ShouldRedirect_WhenLangIsSpanish()
    {
        // Arrange
        var products = new List<Product>();
        _productServiceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(Result.Success<IEnumerable<Product>, TiendaDawWeb.Errors.DomainError>(products));

        // Act
        var result = await InvokeIndex(null, null, null, null, 1, 12, "es");

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Index");
    }

    /// <summary>
    /// PRUEBA: Index establece ViewBag con filtros de búsqueda.
    /// </summary>
    [Test]
    public async Task Index_ShouldSetViewBag_WithSearchFilters()
    {
        // Arrange
        var products = new List<Product>();
        _productServiceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(Result.Success<IEnumerable<Product>, TiendaDawWeb.Errors.DomainError>(products));

        // Act
        var result = await InvokeIndex("test", "LAPTOPS", 100, 500, 1, 12, null);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        _controller.ViewData["Search"].Should().Be("test");
        _controller.ViewData["Categoria"].Should().Be("LAPTOPS");
        _controller.ViewData["MinPrecio"].Should().Be(100f);
        _controller.ViewData["MaxPrecio"].Should().Be(500f);
    }

    /// <summary>
    /// PRUEBA: Index redirige para otros idiomas (fr, de, pt).
    /// </summary>
    [Test]
    [TestCase("fr")]
    [TestCase("de")]
    [TestCase("pt")]
    public async Task Index_ShouldRedirect_ForOtherLanguages(string lang)
    {
        // Arrange
        var products = new List<Product>();
        _productServiceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(Result.Success<IEnumerable<Product>, TiendaDawWeb.Errors.DomainError>(products));

        // Act
        var result = await InvokeIndex(null, null, null, null, 1, 12, lang);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Index");
    }

    /// <summary>
    /// PRUEBA: Index redirige para idioma desconocido.
    /// </summary>
    [Test]
    public async Task Index_ShouldRedirect_ForUnknownLanguage()
    {
        // Arrange
        var products = new List<Product>();
        _productServiceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(Result.Success<IEnumerable<Product>, TiendaDawWeb.Errors.DomainError>(products));

        // Act
        var result = await InvokeIndex(null, null, null, null, 1, 12, "zh");

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Index");
    }

    /// <summary>
    /// Método helper para invocar Index con todos los parámetros.
    /// Esto evita problemas con primary constructors en .NET 10.
    /// </summary>
    private async Task<IActionResult> InvokeIndex(
        string? q, string? categoria, float? minPrecio, float? maxPrecio, 
        int page, int size, string? lang)
    {
        return await _controller.Index(q, categoria, minPrecio, maxPrecio, page, size, lang);
    }
}
