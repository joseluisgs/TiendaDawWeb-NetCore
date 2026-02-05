using FluentAssertions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TiendaDawWeb.Shared.Services.BackgroundServices;

namespace TiendaDawWeb.Tests.Shared.Services.BackgroundServices;

public class BackgroundJobServiceTests
{
    [Test]
    public void BackgroundJobService_CanBeCreated()
    {
        var serviceProviderMock = new Mock<IServiceProvider>();
        var loggerMock = new Mock<ILogger<BackgroundJobService>>();
        var environmentMock = new Mock<IHostEnvironment>();

        var service = new BackgroundJobService(
            serviceProviderMock.Object,
            loggerMock.Object,
            environmentMock.Object);

        service.Should().NotBeNull();
    }

    [Test]
    public void BackgroundJobService_ImplementsBackgroundService()
    {
        var serviceProviderMock = new Mock<IServiceProvider>();
        var loggerMock = new Mock<ILogger<BackgroundJobService>>();
        var environmentMock = new Mock<IHostEnvironment>();

        var service = new BackgroundJobService(
            serviceProviderMock.Object,
            loggerMock.Object,
            environmentMock.Object);

        service.Should().BeAssignableTo<BackgroundService>();
    }

    [Test]
    public void BackgroundJobService_HasRequiredDependencies()
    {
        var serviceProviderMock = new Mock<IServiceProvider>();
        var loggerMock = new Mock<ILogger<BackgroundJobService>>();
        var environmentMock = new Mock<IHostEnvironment>();

        var service = new BackgroundJobService(
            serviceProviderMock.Object,
            loggerMock.Object,
            environmentMock.Object);

        service.Should().NotBeNull();
    }

    [Test]
    public void BackgroundJobService_CanBeDisposed()
    {
        var serviceProviderMock = new Mock<IServiceProvider>();
        var loggerMock = new Mock<ILogger<BackgroundJobService>>();
        var environmentMock = new Mock<IHostEnvironment>();

        var service = new BackgroundJobService(
            serviceProviderMock.Object,
            loggerMock.Object,
            environmentMock.Object);

        service.Should().NotBeNull();
        service.Dispose();
    }
}
