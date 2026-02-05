using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TiendaDawWeb.Shared.Errors;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Models.Enums;
using TiendaDawWeb.Shared.Services.Pdf;
using PurchaseModel = TiendaDawWeb.Shared.Models.Purchase;

namespace TiendaDawWeb.Tests.Shared.Services;

public class PdfServiceTests
{
    private Mock<ILogger<PdfService>> _loggerMock = null!;
    private PdfService _service = null!;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<PdfService>>();
        _service = new PdfService(_loggerMock.Object);
    }

    [Test]
    public async Task GenerateInvoicePdfAsync_GeneratesPdf_Success()
    {
        var purchase = new PurchaseModel
        {
            Id = 1,
            Total = 100,
            FechaCompra = DateTime.UtcNow,
            CompradorId = 1,
            Comprador = new User { Id = 1, Email = "test@test.com", UserName = "test", Nombre = "Test User" },
            Products = new List<Product>
            {
                new Product { Id = 1, Nombre = "Product1", Categoria = ProductCategory.SMARTPHONES, Precio = 100 }
            }
        };

        var result = await _service.GenerateInvoicePdfAsync(purchase);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Length.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task GenerateInvoicePdfAsync_ReturnsPdf_WithMultipleProducts()
    {
        var purchase = new PurchaseModel
        {
            Id = 1,
            Total = 300,
            FechaCompra = DateTime.UtcNow,
            CompradorId = 1,
            Comprador = new User { Id = 1, Email = "test@test.com", UserName = "test", Nombre = "Test User" },
            Products = new List<Product>
            {
                new Product { Id = 1, Nombre = "Product1", Categoria = ProductCategory.SMARTPHONES, Precio = 100 },
                new Product { Id = 2, Nombre = "Product2", Categoria = ProductCategory.LAPTOPS, Precio = 200 }
            }
        };

        var result = await _service.GenerateInvoicePdfAsync(purchase);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Length.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task GenerateInvoicePdfAsync_ReturnsPdf_WithNullComprador()
    {
        var purchase = new PurchaseModel
        {
            Id = 1,
            Total = 100,
            FechaCompra = DateTime.UtcNow,
            CompradorId = 1,
            Products = new List<Product>
            {
                new Product { Id = 1, Nombre = "Product1", Categoria = ProductCategory.SMARTPHONES, Precio = 100 }
            }
        };

        var result = await _service.GenerateInvoicePdfAsync(purchase);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task GenerateInvoicePdfAsync_ReturnsPdf_WithEmptyProducts()
    {
        var purchase = new PurchaseModel
        {
            Id = 1,
            Total = 0,
            FechaCompra = DateTime.UtcNow,
            CompradorId = 1,
            Comprador = new User { Id = 1, Email = "test@test.com", UserName = "test", Nombre = "Test User" },
            Products = new List<Product>()
        };

        var result = await _service.GenerateInvoicePdfAsync(purchase);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNullOrEmpty();
    }
}
