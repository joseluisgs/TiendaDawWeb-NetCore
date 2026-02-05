using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Models.Enums;
using TiendaDawWeb.Shared.Services.Pdf;
using PurchaseModel = TiendaDawWeb.Shared.Models.Purchase;

namespace TiendaDawWeb.Tests.Shared.Services.Pdf;

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
    public async Task GenerateInvoicePdfAsync_ReturnsPdfBytes_WhenPurchaseExists()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test", Nombre = "Test", Apellidos = "User" };
        var purchase = new PurchaseModel
        {
            Id = 1,
            CompradorId = 1,
            Total = 100,
            FechaCompra = DateTime.UtcNow,
            Comprador = user,
            Products = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    Nombre = "Test Product",
                    Descripcion = "Description",
                    Precio = 100,
                    Categoria = ProductCategory.SMARTPHONES,
                    PropietarioId = 2
                }
            }
        };

        var result = await _service.GenerateInvoicePdfAsync(purchase);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Length.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task GenerateInvoicePdfAsync_ReturnsPdf_WithEmptyProducts()
    {
        var purchase = new PurchaseModel
        {
            Id = 1,
            CompradorId = 1,
            Total = 0,
            FechaCompra = DateTime.UtcNow,
            Products = new List<Product>()
        };

        var result = await _service.GenerateInvoicePdfAsync(purchase);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Test]
    public async Task GenerateInvoicePdfAsync_ReturnsPdf_WithMultipleProducts()
    {
        var purchase = new PurchaseModel
        {
            Id = 1,
            CompradorId = 1,
            Total = 300,
            FechaCompra = DateTime.UtcNow,
            Products = new List<Product>
            {
                new Product { Id = 1, Nombre = "Product 1", Precio = 100, Categoria = ProductCategory.SMARTPHONES },
                new Product { Id = 2, Nombre = "Product 2", Precio = 100, Categoria = ProductCategory.LAPTOPS },
                new Product { Id = 3, Nombre = "Product 3", Precio = 100, Categoria = ProductCategory.AUDIO }
            }
        };

        var result = await _service.GenerateInvoicePdfAsync(purchase);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Length.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task GenerateInvoicePdfAsync_ReturnsPdf_WithDecimalTotal()
    {
        var purchase = new PurchaseModel
        {
            Id = 1,
            CompradorId = 1,
            Total = 99.99m,
            FechaCompra = DateTime.UtcNow,
            Products = new List<Product>
            {
                new Product { Id = 1, Nombre = "Test", Precio = 99.99m, Categoria = ProductCategory.GAMING }
            }
        };

        var result = await _service.GenerateInvoicePdfAsync(purchase);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }
}
