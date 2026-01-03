using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using TiendaDawWeb.Models;
using TiendaDawWeb.Services.Implementations;
using TiendaDawWeb.Models.Enums;

namespace TiendaDawWeb.Tests.Services;

/// <summary>
/// OBJETIVO: Validar la generación de documentos PDF (facturas).
/// LO QUE BUSCA: Confirmar que el servicio genera un flujo de bytes válido
/// y maneja correctamente casos de datos incompletos.
/// </summary>
[TestFixture]
public class PdfServiceTests
{
    private Mock<ILogger<PdfService>> _loggerMock;
    private PdfService _pdfService;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<PdfService>>();
        _pdfService = new PdfService(_loggerMock.Object);
    }

    /// <summary>
    /// PRUEBA: Generación de factura válida.
    /// OBJETIVO: Verificar que se genera un PDF con contenido cuando los datos de compra son correctos.
    /// </summary>
    [Test]
    public async Task GenerateInvoicePdfAsync_ShouldGeneratePdf_WhenPurchaseIsValid()
    {
        // Arrange
        var purchase = new Purchase
        {
            Id = 1,
            FechaCompra = DateTime.Now,
            Total = 121, // 100 + 21% IVA
            Comprador = new User { Nombre = "John", Apellidos = "Doe", Email = "john@example.com", UserName = "johndoe" },
            Products = new List<Product>
            {
                new Product { Nombre = "Product 1", Precio = 100, Categoria = ProductCategory.SMARTPHONES }
            }
        };

        // Act
        var result = await _pdfService.GenerateInvoicePdfAsync(purchase);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().NotBeEmpty();
        result.Value.Length.Should().BeGreaterThan(0);
    }

    /// <summary>
    /// PRUEBA: Generación de factura sin datos de comprador.
    /// OBJETIVO: Validar la robustez del servicio ante datos nulos opcionales.
    /// </summary>
    [Test]
    public async Task GenerateInvoicePdfAsync_ShouldHandleMissingBuyerInfo()
    {
        // Arrange
        var purchase = new Purchase
        {
            Id = 2,
            FechaCompra = DateTime.Now,
            Total = 50,
            Comprador = null!, // Missing buyer
            Products = new List<Product>
            {
                new Product { Nombre = "Product 2", Precio = 50, Categoria = ProductCategory.ACCESSORIES }
            }
        };

        // Act
        var result = await _pdfService.GenerateInvoicePdfAsync(purchase);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }
}
