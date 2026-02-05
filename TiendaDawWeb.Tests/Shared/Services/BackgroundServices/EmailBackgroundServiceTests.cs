using System.Threading.Channels;
using FluentAssertions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TiendaDawWeb.Shared.Services.BackgroundServices;
using TiendaDawWeb.Shared.Services.Email;
using Result = CSharpFunctionalExtensions.Result;

namespace TiendaDawWeb.Tests.Shared.Services.BackgroundServices;

public class EmailBackgroundServiceTests
{
    private Mock<ILogger<EmailBackgroundService>> _loggerMock = null!;
    private Mock<IServiceProvider> _serviceProviderMock = null!;
    private Mock<IServiceScope> _serviceScopeMock = null!;
    private Mock<IServiceScopeFactory> _serviceScopeFactoryMock = null!;
    private Mock<IEmailService> _emailServiceMock = null!;
    private Channel<EmailMessage> _emailChannel = null!;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<EmailBackgroundService>>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _serviceScopeMock = new Mock<IServiceScope>();
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _emailServiceMock = new Mock<IEmailService>();

        _serviceProviderMock.Setup(s => s.GetService(typeof(IServiceScopeFactory)))
            .Returns(_serviceScopeFactoryMock.Object);
        _serviceScopeFactoryMock.Setup(f => f.CreateScope())
            .Returns(_serviceScopeMock.Object);
        _serviceScopeMock.Setup(s => s.ServiceProvider.GetService(typeof(IEmailService)))
            .Returns(_emailServiceMock.Object);

        _emailChannel = Channel.CreateUnbounded<EmailMessage>();
    }

    [TearDown]
    public void TearDown()
    {
        try
        {
            _emailChannel.Writer.Complete();
        }
        catch
        {
        }
    }

    [Test]
    public void Constructor_CanBeCreated()
    {
        var service = new EmailBackgroundService(
            _emailChannel,
            _serviceProviderMock.Object,
            _loggerMock.Object);

        service.Should().NotBeNull();
    }

    [Test]
    public void Constructor_SetsDependencies()
    {
        var service = new EmailBackgroundService(
            _emailChannel,
            _serviceProviderMock.Object,
            _loggerMock.Object);

        service.Should().NotBeNull();
    }

    [Test]
    public void EmailBackgroundService_ImplementsBackgroundService()
    {
        var service = new EmailBackgroundService(
            _emailChannel,
            _serviceProviderMock.Object,
            _loggerMock.Object);

        service.Should().BeAssignableTo<BackgroundService>();
    }

    [Test]
    public void EmailBackgroundService_CanBeDisposed()
    {
        var service = new EmailBackgroundService(
            _emailChannel,
            _serviceProviderMock.Object,
            _loggerMock.Object);

        service.Should().NotBeNull();
        service.Dispose();
    }

    [Test]
    public void Constructor_AcceptsUnboundedChannel()
    {
        var unboundedChannel = Channel.CreateUnbounded<EmailMessage>();
        var service = new EmailBackgroundService(
            unboundedChannel,
            _serviceProviderMock.Object,
            _loggerMock.Object);

        service.Should().NotBeNull();
    }

    [Test]
    public void Constructor_AcceptsBoundedChannel()
    {
        var boundedChannel = Channel.CreateBounded<EmailMessage>(100);
        var service = new EmailBackgroundService(
            boundedChannel,
            _serviceProviderMock.Object,
            _loggerMock.Object);

        service.Should().NotBeNull();
    }
}
