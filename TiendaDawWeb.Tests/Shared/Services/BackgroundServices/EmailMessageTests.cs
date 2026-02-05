using FluentAssertions;
using TiendaDawWeb.Shared.Services.Email;

namespace TiendaDawWeb.Tests.Shared.Services.BackgroundServices;

public class EmailMessageTests
{
    [Test]
    public void Constructor_WithRequiredProperties_CreatesMessage()
    {
        var message = new EmailMessage
        {
            To = "test@example.com",
            Subject = "Test Subject",
            Body = "Test Body"
        };

        message.To.Should().Be("test@example.com");
        message.Subject.Should().Be("Test Subject");
        message.Body.Should().Be("Test Body");
    }

    [Test]
    public void PropertySetters_WorkCorrectly()
    {
        var message = new EmailMessage
        {
            To = "test@example.com",
            Subject = "Test Subject",
            Body = "Test Body",
            Attachment = new byte[] { 1, 2, 3 },
            AttachmentName = "test.pdf",
            IsHtml = true
        };

        message.To.Should().Be("test@example.com");
        message.Subject.Should().Be("Test Subject");
        message.Body.Should().Be("Test Body");
        message.Attachment.Should().BeEquivalentTo(new byte[] { 1, 2, 3 });
        message.AttachmentName.Should().Be("test.pdf");
        message.IsHtml.Should().BeTrue();
    }

    [Test]
    public void DefaultIsHtml_IsFalse()
    {
        var message = new EmailMessage
        {
            To = "test@example.com",
            Subject = "Test Subject",
            Body = "Test Body"
        };

        message.IsHtml.Should().BeFalse();
    }

    [Test]
    public void Attachment_CanBeNull()
    {
        var message = new EmailMessage
        {
            To = "test@example.com",
            Subject = "Test Subject",
            Body = "Test Body",
            Attachment = null
        };

        message.Attachment.Should().BeNull();
    }

    [Test]
    public void AttachmentName_CanBeNull()
    {
        var message = new EmailMessage
        {
            To = "test@example.com",
            Subject = "Test Subject",
            Body = "Test Body",
            AttachmentName = null
        };

        message.AttachmentName.Should().BeNull();
    }

    [Test]
    public void IsHtml_CanBeSetToTrue()
    {
        var message = new EmailMessage
        {
            To = "test@example.com",
            Subject = "Test Subject",
            Body = "<html>Test</html>",
            IsHtml = true
        };

        message.IsHtml.Should().BeTrue();
    }

    [Test]
    public void AllProperties_CanBeInitialized()
    {
        var message = new EmailMessage
        {
            To = "recipient@example.com",
            Subject = "Complete Test",
            Body = "<html><body>Test</body></html>",
            IsHtml = true,
            Attachment = new byte[] { 0x25, 0x50, 0x44, 0x46 },
            AttachmentName = "document.pdf"
        };

        message.To.Should().Be("recipient@example.com");
        message.Subject.Should().Be("Complete Test");
        message.Body.Should().Contain("Test");
        message.IsHtml.Should().BeTrue();
        message.Attachment.Should().NotBeNull();
        message.AttachmentName.Should().Be("document.pdf");
    }
}
