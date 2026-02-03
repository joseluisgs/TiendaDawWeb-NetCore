using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using TiendaDawWeb.Controllers;
using FluentAssertions;

namespace TiendaDawWeb.Tests.Controllers;

/// <summary>
/// OBJETIVO: Validar el comportamiento del controlador raíz.
/// LO QUE BUSCA: Asegurar que las redirecciones funcionan correctamente.
/// </summary>
[TestFixture]
public class HomeControllerTests
{
    private HomeController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _controller = new HomeController();
    }

    [TearDown]
    public void TearDown()
    {
        _controller?.Dispose();
    }

    /// <summary>
    /// PRUEBA: Index redirige a Public con parámetros preservados.
    /// </summary>
    [Test]
    public void Index_ShouldRedirectToPublic_WithSearchQuery()
    {
        // Act
        var result = _controller.Index(search: "test", q: null, categoria: null, minPrecio: null, maxPrecio: null);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Index");
        redirectResult.ControllerName.Should().Be("Public");
        redirectResult.RouteValues?["q"].Should().Be("test");
    }

    /// <summary>
    /// PRUEBA: Index usa parámetro q si search es null.
    /// </summary>
    [Test]
    public void Index_ShouldUseQParameter_WhenSearchIsNull()
    {
        // Act
        var result = _controller.Index(search: null, q: "keyword", categoria: null, minPrecio: null, maxPrecio: null);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.RouteValues?["q"].Should().Be("keyword");
    }

    /// <summary>
    /// PRUEBA: Index preserva parámetros de categoría.
    /// </summary>
    [Test]
    public void Index_ShouldPreserveCategoriaParameter()
    {
        // Act
        var result = _controller.Index(search: null, q: null, categoria: "SMARTPHONES", minPrecio: null, maxPrecio: null);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.RouteValues?["categoria"].Should().Be("SMARTPHONES");
    }

    /// <summary>
    /// PRUEBA: Index preserva parámetros de precio.
    /// </summary>
    [Test]
    public void Index_ShouldPreservePrecioParameters()
    {
        // Act
        var result = _controller.Index(search: null, q: null, categoria: null, minPrecio: 100, maxPrecio: 500);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.RouteValues?["minPrecio"].Should().Be(100f);
        redirectResult.RouteValues?["maxPrecio"].Should().Be(500f);
    }

    /// <summary>
    /// PRUEBA: Index preserva parámetros de paginación.
    /// </summary>
    [Test]
    public void Index_ShouldPreservePaginationParameters()
    {
        // Act
        var result = _controller.Index(search: null, q: null, categoria: null, minPrecio: null, maxPrecio: null, page: 2, size: 24);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.RouteValues?["page"].Should().Be(2);
        redirectResult.RouteValues?["size"].Should().Be(24);
    }

    /// <summary>
    /// PRUEBA: Index combina todos los parámetros correctamente.
    /// </summary>
    [Test]
    public void Index_ShouldCombineAllParameters()
    {
        // Act
        var result = _controller.Index(
            search: "laptop",
            q: null,
            categoria: "LAPTOPS",
            minPrecio: 500,
            maxPrecio: 1500,
            page: 3,
            size: 12);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ControllerName.Should().Be("Public");
        redirectResult.ActionName.Should().Be("Index");
        redirectResult.RouteValues?["q"].Should().Be("laptop");
        redirectResult.RouteValues?["categoria"].Should().Be("LAPTOPS");
        redirectResult.RouteValues?["minPrecio"].Should().Be(500f);
        redirectResult.RouteValues?["maxPrecio"].Should().Be(1500f);
        redirectResult.RouteValues?["page"].Should().Be(3);
        redirectResult.RouteValues?["size"].Should().Be(12);
    }

    /// <summary>
    /// PRUEBA: Index con parámetros vacíos redirige correctamente.
    /// </summary>
    [Test]
    public void Index_ShouldRedirectWithNullParameters_WhenEmpty()
    {
        // Act
        var result = _controller.Index(search: null, q: null, categoria: null, minPrecio: null, maxPrecio: null);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.RouteValues?["q"].Should().BeNull();
        redirectResult.RouteValues?["categoria"].Should().BeNull();
        redirectResult.RouteValues?["page"].Should().Be(1);
        redirectResult.RouteValues?["size"].Should().Be(12);
    }
}
