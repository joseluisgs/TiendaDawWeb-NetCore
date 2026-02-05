using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Channels;
using TiendaDawWeb.Shared.Services.Email;

namespace TiendaDawWeb.Tests.Shared.Services.Email;

public class EmailServiceTests
{
    private Mock<IConfiguration> _configMock = null!;
    private Mock<ILogger<EmailService>> _loggerMock = null!;
    private Channel<EmailMessage> _emailChannel = null!;
    private EmailService _service = null!;

    [SetUp]
    public void Setup()
    {
        _configMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<EmailService>>();
        _emailChannel = Channel.CreateUnbounded<EmailMessage>();
        _service = new EmailService(_configMock.Object, _loggerMock.Object, _emailChannel);
    }

    [TearDown]
    public void TearDown()
    {
        _emailChannel.Writer.Complete();
    }

    [Test]
    public async Task SendWelcomeEmailAsync_ReturnsSuccess_WhenNoSmtpConfig()
    {
        _configMock.Setup(x => x["Email:SmtpHost"]).Returns((string)null!);
        _configMock.Setup(x => x["Email:SmtpUser"]).Returns((string)null!);

        var result = await _service.SendWelcomeEmailAsync("test@example.com", "TestUser");

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task SendWelcomeEmailAsync_ReturnsSuccess_WithMockedConfig()
    {
        _configMock.Setup(x => x["Email:SmtpHost"]).Returns("${SmtpHost}");
        _configMock.Setup(x => x["Email:SmtpUser"]).Returns("${SmtpUser}");

        var result = await _service.SendWelcomeEmailAsync("test@example.com", "TestUser");

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task SendPurchaseConfirmationEmailAsync_ReturnsSuccess_WithProducts()
    {
        _configMock.Setup(x => x["Email:SmtpHost"]).Returns((string)null!);
        _configMock.Setup(x => x["Email:SmtpUser"]).Returns((string)null!);

        var purchase = new TiendaDawWeb.Shared.Models.Purchase
        {
            Id = 1,
            Total = 100,
            Products = new List<TiendaDawWeb.Shared.Models.Product>
            {
                new TiendaDawWeb.Shared.Models.Product
                {
                    Nombre = "Test Product",
                    Categoria = TiendaDawWeb.Shared.Models.Enums.ProductCategory.SMARTPHONES,
                    Precio = 100
                }
            }
        };

        var result = await _service.SendPurchaseConfirmationEmailAsync("test@example.com", purchase, null);

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task SendPurchaseConfirmationEmailAsync_WithPdf_ReturnsSuccess()
    {
        _configMock.Setup(x => x["Email:SmtpHost"]).Returns((string)null!);
        _configMock.Setup(x => x["Email:SmtpUser"]).Returns((string)null!);

        var purchase = new TiendaDawWeb.Shared.Models.Purchase
        {
            Id = 1,
            Total = 100,
            Products = new List<TiendaDawWeb.Shared.Models.Product>()
        };
        var pdfBytes = new byte[] { 1, 2, 3, 4, 5 };

        var result = await _service.SendPurchaseConfirmationEmailAsync("test@example.com", purchase, pdfBytes);

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task SendEmailAsync_ReturnsSuccess_WhenNoSmtpConfig()
    {
        _configMock.Setup(x => x["Email:SmtpHost"]).Returns((string)null!);
        _configMock.Setup(x => x["Email:SmtpUser"]).Returns((string)null!);

        var result = await _service.SendEmailAsync("test@example.com", "Test Subject", "Test Body");

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task SendEmailAsync_WithAttachment_ReturnsSuccess()
    {
        _configMock.Setup(x => x["Email:SmtpHost"]).Returns((string)null!);
        _configMock.Setup(x => x["Email:SmtpUser"]).Returns((string)null!);

        var attachment = new byte[] { 1, 2, 3, 4, 5 };

        var result = await _service.SendEmailAsync(
            "test@example.com",
            "Test Subject",
            "Test Body",
            attachment,
            "test.pdf");

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public void EnqueueEmail_DoesNotThrow()
    {
        var action = () => _service.EnqueueEmail(new EmailMessage
        {
            To = "test@example.com",
            Subject = "Test",
            Body = "Test body"
        });

        action.Should().NotThrow();
    }

    [Test]
    public void EnqueueEmail_WithAttachment_DoesNotThrow()
    {
        var action = () => _service.EnqueueEmail(new EmailMessage
        {
            To = "test@example.com",
            Subject = "Test",
            Body = "Test body",
            Attachment = new byte[] { 1, 2, 3 },
            AttachmentName = "test.pdf"
        });

        action.Should().NotThrow();
    }

    [Test]
    public async Task SendWelcomeEmailAsync_HandlesEmptyEmail()
    {
        _configMock.Setup(x => x["Email:SmtpHost"]).Returns((string)null!);

        var result = await _service.SendWelcomeEmailAsync("", "TestUser");

        result.IsSuccess.Should().BeTrue();
    }
}
