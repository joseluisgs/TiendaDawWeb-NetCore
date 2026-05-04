using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TiendaDawWeb.Shared.Blazor.Admin.Charts;
using TiendaDawWeb.Shared.Services.Stats;

namespace TiendaDawWeb.Tests.Blazor;

public class TopParticipantesChartTests
{
    private readonly Mock<IStatisticsService> _mockStatsService;
    private readonly Mock<ILogger<TopParticipantesChart>> _mockLogger;

    public TopParticipantesChartTests()
    {
        _mockStatsService = new Mock<IStatisticsService>();
        _mockLogger = new Mock<ILogger<TopParticipantesChart>>();
    }

    [Test]
    public void TopParticipantesChart_Component_CanBeCreated()
    {
#pragma warning disable CS0618
        using var ctx = new Bunit.TestContext();
#pragma warning restore CS0618
        ctx.Services.AddSingleton(_mockStatsService.Object);
        ctx.Services.AddSingleton(_mockLogger.Object);

        var cut = ctx.Render<TopParticipantesChart>();

        cut.Should().NotBeNull();
    }

    [Test]
    public void TopParticipantesChart_ImplementsIDisposable()
    {
        typeof(TopParticipantesChart).Should().Implement<IDisposable>();
    }

    [Test]
    public void TopParticipantesChart_HasCardStructure()
    {
#pragma warning disable CS0618
        using var ctx = new Bunit.TestContext();
#pragma warning restore CS0618
        ctx.Services.AddSingleton(_mockStatsService.Object);
        ctx.Services.AddSingleton(_mockLogger.Object);

        var cut = ctx.Render<TopParticipantesChart>();

        cut.Find(".card").Should().NotBeNull();
    }

    [Test]
    public void TopParticipantesChart_HasInfoHeader()
    {
#pragma warning disable CS0618
        using var ctx = new Bunit.TestContext();
#pragma warning restore CS0618
        ctx.Services.AddSingleton(_mockStatsService.Object);
        ctx.Services.AddSingleton(_mockLogger.Object);

        var cut = ctx.Render<TopParticipantesChart>();

        cut.Find(".card-header.bg-info").Should().NotBeNull();
    }

    [Test]
    public void TopParticipantesChart_HasModeToggleButtons()
    {
#pragma warning disable CS0618
        using var ctx = new Bunit.TestContext();
#pragma warning restore CS0618
        ctx.Services.AddSingleton(_mockStatsService.Object);
        ctx.Services.AddSingleton(_mockLogger.Object);

        var cut = ctx.Render<TopParticipantesChart>();

        cut.FindAll("button").Should().NotBeEmpty();
    }

    [Test]
    public void TopParticipantesChart_HasRefreshButton()
    {
#pragma warning disable CS0618
        using var ctx = new Bunit.TestContext();
#pragma warning restore CS0618
        ctx.Services.AddSingleton(_mockStatsService.Object);
        ctx.Services.AddSingleton(_mockLogger.Object);

        var cut = ctx.Render<TopParticipantesChart>();

        cut.FindAll("button").Count.Should().BeGreaterThan(1);
    }
}