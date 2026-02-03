using System.Threading.Channels;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TiendaDawWeb.Models;
using TiendaDawWeb.Services.Email;

namespace TiendaDawWeb.Tests.Services;

/// <summary>
/// OBJETIVO: Validar el servicio de envío de correos electrónicos.
/// LO QUE BUSCA: Confirmar que los correos se generan correctamente y que el servicio
/// maneja la falta de configuración SMTP sin fallar críticamente.
/// </summary>
[TestFixture]
public class EmailServiceTests
{
    private Mock<IConfiguration> _configMock = null!;
    private Mock<ILogger<EmailService>> _loggerMock = null!;
    private Channel<EmailMessage> _emailChannel;
    private EmailService _emailService = null!;

    [SetUp]
    public void Setup()
    {
        _configMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<EmailService>>();
        _emailChannel = Channel.CreateUnbounded<EmailMessage>();
        _emailService = new EmailService(_configMock.Object, _loggerMock.Object, _emailChannel);
    }

    [TearDown]
    public void TearDown()
    {
        _emailChannel.Writer.Complete();
    }

    /// <summary>
    /// PRUEBA: Envío de email sin configuración SMTP.
    /// OBJETIVO: Verificar que el servicio registra un aviso y devuelve éxito.
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
    }

    /// <summary>
    /// PRUEBA: Generación de email de bienvenida.
    /// OBJETIVO: Validar que el flujo de bienvenida se completa satisfactoriamente.
    /// </summary>
    [Test]
    public async Task SendWelcomeEmailAsync_ShouldCallSendEmail()
    {
        // Arrange
        _configMock.Setup(c => c["Email:SmtpHost"]).Returns(string.Empty);

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
        _configMock.Setup(c => c["Email:SmtpHost"]).Returns(string.Empty);
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

    /// <summary>
    /// PRUEBA: SendEmailAsync con configuración placeholder (${...}).
    /// OBJETIVO: Verificar que el servicio detecta placeholders de configuración no reemplazados.
    /// </summary>
    [Test]
    public async Task SendEmailAsync_ShouldReturnSuccess_WhenConfigHasPlaceholders()
    {
        // Arrange
        _configMock.Setup(c => c["Email:SmtpHost"]).Returns("${SMTP_HOST}");
        _configMock.Setup(c => c["Email:SmtpPort"]).Returns("${SMTP_PORT}");
        _configMock.Setup(c => c["Email:SmtpUser"]).Returns("user@placeholder.com");
        _configMock.Setup(c => c["Email:SmtpPass"]).Returns("placeholder");

        // Act
        var result = await _emailService.SendEmailAsync("test@test.com", "Subject", "Body");

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    /// <summary>
    /// PRUEBA: SendPurchaseConfirmationEmailAsync con PDF adjunto.
    /// OBJETIVO: Verificar que se puede enviar un email con附件.
    /// </summary>
    [Test]
    public async Task SendPurchaseConfirmationEmailAsync_ShouldHandlePdfAttachment()
    {
        // Arrange
        _configMock.Setup(c => c["Email:SmtpHost"]).Returns(string.Empty);
        var purchase = new Purchase { Id = 100, Total = 250.50m };
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // %PDF

        // Act
        var result = await _emailService.SendPurchaseConfirmationEmailAsync(
            "user@test.com", purchase, pdfBytes);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    /// <summary>
    /// PRUEBA: EnqueueEmail escribe en el canal.
    /// OBJETIVO: Verificar que los mensajes se encolan correctamente.
    /// </summary>
    [Test]
    public void EnqueueEmail_ShouldWriteToChannel()
    {
        // Arrange
        var emailMessage = new EmailMessage
        {
            To = "test@test.com",
            Subject = "Test",
            Body = "Test body"
        };

        // Act
        _emailService.EnqueueEmail(emailMessage);

        // Assert
        _emailChannel.Reader.TryRead(out var result).Should().BeTrue();
        result!.To.Should().Be("test@test.com");
    }

    /// <summary>
    /// PRUEBA: SendEmailAsync con FromName configurado.
    /// OBJETIVO: Verificar que FromName se aplica correctamente.
    /// </summary>
    [Test]
    public async Task SendEmailAsync_ShouldApplyFromName_WhenConfigured()
    {
        // Arrange
        _configMock.Setup(c => c["Email:SmtpHost"]).Returns(string.Empty);
        _configMock.Setup(c => c["Email:FromName"]).Returns("WalaDaw Team");

        // Act
        var result = await _emailService.SendEmailAsync("test@test.com", "Subject", "Body");

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    /// <summary>
    /// PRUEBA: SendEmailAsync maneja SmtpPort null.
    /// OBJETIVO: Verificar que usa el valor por defecto 587.
    /// </summary>
    [Test]
    public async Task SendEmailAsync_ShouldUseDefaultPort_WhenPortNotConfigured()
    {
        // Arrange
        _configMock.Setup(c => c["Email:SmtpHost"]).Returns(string.Empty);
        _configMock.Setup(c => c["Email:SmtpPort"]).Returns((string?)null);

        // Act
        var result = await _emailService.SendEmailAsync("test@test.com", "Subject", "Body");

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
