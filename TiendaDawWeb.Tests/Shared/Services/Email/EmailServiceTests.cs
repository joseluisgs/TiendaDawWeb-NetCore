using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TiendaDawWeb.Shared.Errors;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Models.Enums;
using TiendaDawWeb.Shared.Services.Email;
using System.Threading.Channels;
using PurchaseModel = TiendaDawWeb.Shared.Models.Purchase;

namespace TiendaDawWeb.Tests.Shared.Services;

public class EmailServiceTests
{
    private Mock<IConfiguration> _configurationMock = null!;
    private Mock<ILogger<EmailService>> _loggerMock = null!;
    private Channel<EmailMessage> _emailChannel = null!;
    private EmailService _service = null!;

    [SetUp]
    public void Setup()
    {
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<EmailService>>();
        _emailChannel = Channel.CreateUnbounded<EmailMessage>();
        _service = new EmailService(_configurationMock.Object, _loggerMock.Object, _emailChannel);
    }

    [TearDown]
    public void TearDown()
    {
        _emailChannel.Writer.Complete();
    }

    #region SendWelcomeEmailAsync Tests

    [Test]
    public async Task SendWelcomeEmailAsync_ReturnsSuccess_WhenSmtpNotConfigured()
    {
        _configurationMock.Setup(c => c["Email:SmtpHost"]).Returns((string)null!);

        var result = await _service.SendWelcomeEmailAsync("test@example.com", "TestUser");

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task SendWelcomeEmailAsync_ReturnsSuccess_WhenSmtpHostIsPlaceholder()
    {
        _configurationMock.Setup(c => c["Email:SmtpHost"]).Returns("${SmtpHost}");

        var result = await _service.SendWelcomeEmailAsync("test@example.com", "TestUser");

        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region SendPurchaseConfirmationEmailAsync Tests

    [Test]
    public async Task SendPurchaseConfirmationEmailAsync_ReturnsSuccess_WhenSmtpNotConfigured()
    {
        _configurationMock.Setup(c => c["Email:SmtpHost"]).Returns((string)null!);

        var purchase = new PurchaseModel
        {
            Id = 1,
            Total = 100,
            FechaCompra = DateTime.UtcNow,
            CompradorId = 1,
            Products = new List<Product>()
        };

        var result = await _service.SendPurchaseConfirmationEmailAsync("test@example.com", purchase);

        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region SendEmailAsync Tests

    [Test]
    public async Task SendEmailAsync_ReturnsSuccess_WhenSmtpNotConfigured()
    {
        _configurationMock.Setup(c => c["Email:SmtpHost"]).Returns((string)null!);

        var result = await _service.SendEmailAsync(
            "test@example.com",
            "Test Subject",
            "<html><body>Test Body</body></html>"
        );

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task SendEmailAsync_ReturnsSuccess_WhenSmtpHostIsPlaceholder()
    {
        _configurationMock.Setup(c => c["Email:SmtpHost"]).Returns("${SmtpHost}");

        var result = await _service.SendEmailAsync(
            "test@example.com",
            "Test Subject",
            "<html><body>Test Body</body></html>"
        );

        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region EnqueueEmail Tests

    [Test]
    public void EnqueueEmail_EnqueuesMessage_Success()
    {
        var emailMessage = new EmailMessage
        {
            To = "test@example.com",
            Subject = "Test Subject",
            Body = "Test Body"
        };

        _service.EnqueueEmail(emailMessage);

        _emailChannel.Reader.TryRead(out var dequeued).Should().BeTrue();
        dequeued!.To.Should().Be("test@example.com");
        dequeued.Subject.Should().Be("Test Subject");
    }

    #endregion
}
