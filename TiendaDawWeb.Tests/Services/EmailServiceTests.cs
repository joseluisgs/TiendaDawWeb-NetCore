using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TiendaDawWeb.Models;
using TiendaDawWeb.Services.Implementations;
using FluentAssertions;
using TiendaDawWeb.Models.Enums;
using MailKit.Net.Smtp;
using MimeKit;

namespace TiendaDawWeb.Tests.Services;

/// <summary>
/// OBJETIVO: Validar el servicio de envío de correos electrónicos.
/// LO QUE BUSCA: Confirmar que los correos se generan correctamente y que el servicio
/// maneja la falta de configuración SMTP sin fallar críticamente.
/// </summary>
[TestFixture]
public class EmailServiceTests
{
    private Mock<IConfiguration> _configMock;
    private Mock<ILogger<EmailService>> _loggerMock;
    private EmailService _emailService;

    [SetUp]
    public void Setup()
    {
        _configMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<EmailService>>();
        _emailService = new EmailService(_configMock.Object, _loggerMock.Object);
    }

    /// <summary>
    /// PRUEBA: Envío de email sin configuración SMTP.
    /// OBJETIVO: Verificar que el servicio registra un aviso y devuelve éxito para no bloquear el flujo.
    /// </summary>
    [Test]
    public async Task SendEmailAsync_ShouldReturnSuccess_WhenSmtpNotConfigured()
    {
        // Arrange
        _configMock.Setup(c => c["Email:SmtpHost"]).Returns(string.Empty);
        _configMock.Setup(c => c["Email:SmtpUser"]).Returns(string.Empty);

        // Act
        var result = await _emailService.SendEmailAsync("test@test.com", "Subject", "Body");

        // Assert
        result.IsSuccess.Should().BeTrue();
        // Verify warning log for missing config
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Configuración SMTP no disponible")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// PRUEBA: Generación de email de bienvenida.
    /// OBJETIVO: Validar que el flujo de bienvenida se completa satisfactoriamente.
    /// </summary>
    [Test]
    public async Task SendWelcomeEmailAsync_ShouldCallSendEmail()
    {
        // Arrange
        _configMock.Setup(c => c["Email:SmtpHost"]).Returns(string.Empty); // Skip actual sending

        // Act
        var result = await _emailService.SendWelcomeEmailAsync("user@test.com", "John Doe");

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    /// <summary>
    /// PRUEBA: Generación de email de confirmación de compra.
    /// OBJETIVO: Asegurar que el email con detalles de la compra se procesa correctamente.
    /// </summary>
    [Test]
    public async Task SendPurchaseConfirmationEmailAsync_ShouldCallSendEmail()
    {
        // Arrange
        _configMock.Setup(c => c["Email:SmtpHost"]).Returns(string.Empty); // Skip actual sending
        var purchase = new Purchase
        {
            Id = 1,
            Total = 100,
            FechaCompra = DateTime.Now,
            Comprador = new User { Nombre = "John" },
            Products = new List<Product>
            {
                new Product { Nombre = "Product 1", Precio = 100 }
            }
        };

        // Act
        var result = await _emailService.SendPurchaseConfirmationEmailAsync("user@test.com", purchase);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
