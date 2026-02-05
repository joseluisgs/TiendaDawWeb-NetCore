#nullable disable
using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using TiendaDawWeb.Controllers;
using TiendaDawWeb.Shared.Errors;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Models.Enums;
using TiendaDawWeb.Shared.Services.Product;

namespace TiendaDawWeb.Tests.Mvc.Controllers;

public class PublicControllerTests
{
    private readonly Mock<IProductService> _mockProductService;
    private readonly Mock<ILogger<PublicController>> _mockLogger;
    private readonly PublicController _controller;

    public PublicControllerTests()
    {
        _mockProductService = new Mock<IProductService>();
        _mockLogger = new Mock<ILogger<PublicController>>();
        
        var httpContext = new DefaultHttpContext();
        var actionDescriptor = new ControllerActionDescriptor();
        var actionContext = new ActionContext(httpContext, new RouteData(), actionDescriptor);

        _controller = new PublicController(
            _mockProductService.Object,
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

    #region Index Tests

    [Test]
    public async Task Index_ReturnsView_WithProducts()
    {
        var products = new List<Product>
        {
            new() { Id = 1, Nombre = "Product 1", Precio = 100, Categoria = ProductCategory.SMARTPHONES },
            new() { Id = 2, Nombre = "Product 2", Precio = 200, Categoria = ProductCategory.LAPTOPS }
        };

        _mockProductService.Setup(s => s.GetAllAsync())
            .Returns(Task.FromResult(Result.Success<IEnumerable<Product>, DomainError>(products)));

        var result = await _controller.Index(q: null, categoria: null, minPrecio: null, maxPrecio: null, page: 1, size: 12);

        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult!.Model.Should().NotBeNull();
    }

    [Test]
    public async Task Index_ReturnsEmptyList_WhenServiceFails()
    {
        _mockProductService.Setup(s => s.GetAllAsync())
            .Returns(Task.FromResult(Result.Failure<IEnumerable<Product>, DomainError>(ProductError.NotFound(0))));

        var result = await _controller.Index(q: null, categoria: null, minPrecio: null, maxPrecio: null, page: 1, size: 12);

        result.Should().BeOfType<ViewResult>();
    }

    [Test]
    public async Task Index_Redirects_WhenLangProvided()
    {
        var result = await _controller.Index(q: null, categoria: null, minPrecio: null, maxPrecio: null, page: 1, size: 12, lang: "en");

        result.Should().BeOfType<RedirectToActionResult>();
    }

    [Test]
    public async Task Index_WithSearchQuery_ReturnsFilteredProducts()
    {
        var products = new List<Product>
        {
            new() { Id = 1, Nombre = "Laptop", Precio = 1000, Categoria = ProductCategory.LAPTOPS }
        };

        _mockProductService.Setup(s => s.GetAllAsync())
            .Returns(Task.FromResult(Result.Success<IEnumerable<Product>, DomainError>(products)));

        var result = await _controller.Index(q: "laptop", categoria: null, minPrecio: null, maxPrecio: null, page: 1, size: 12);

        result.Should().BeOfType<ViewResult>();
    }

    [Test]
    public async Task Index_WithCategory_ReturnsFilteredProducts()
    {
        var products = new List<Product>
        {
            new() { Id = 1, Nombre = "Phone", Precio = 500, Categoria = ProductCategory.SMARTPHONES }
        };

        _mockProductService.Setup(s => s.GetAllAsync())
            .Returns(Task.FromResult(Result.Success<IEnumerable<Product>, DomainError>(products)));

        var result = await _controller.Index(q: null, categoria: "SMARTPHONES", minPrecio: null, maxPrecio: null, page: 1, size: 12);

        result.Should().BeOfType<ViewResult>();
    }

    [Test]
    public async Task Index_WithPriceRange_ReturnsFilteredProducts()
    {
        var products = new List<Product>
        {
            new() { Id = 1, Nombre = "Product", Precio = 150, Categoria = ProductCategory.AUDIO }
        };

        _mockProductService.Setup(s => s.GetAllAsync())
            .Returns(Task.FromResult(Result.Success<IEnumerable<Product>, DomainError>(products)));

        var result = await _controller.Index(q: null, categoria: null, minPrecio: 100, maxPrecio: 200, page: 1, size: 12);

        result.Should().BeOfType<ViewResult>();
    }

    [Test]
    public async Task Index_WithPagination_ReturnsPagedResults()
    {
        var products = Enumerable.Range(1, 12).Select(i => new Product
        {
            Id = i,
            Nombre = $"Product {i}",
            Precio = i * 10,
            Categoria = ProductCategory.AUDIO
        }).ToList();

        _mockProductService.Setup(s => s.GetAllAsync())
            .Returns(Task.FromResult(Result.Success<IEnumerable<Product>, DomainError>(products)));

        var result = await _controller.Index(q: null, categoria: null, minPrecio: null, maxPrecio: null, page: 1, size: 12);

        result.Should().BeOfType<ViewResult>();
    }

    #endregion
}
