using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TiendaDawWeb.Shared.Blazor.Admin.Charts;
using TiendaDawWeb.Shared.Services.Stats;
using TestContext = Bunit.BunitContext;

namespace TiendaDawWeb.Tests.Blazor;

public class TopParticipantesChartTests {
    private readonly Mock<ILogger<TopParticipantesChart>> _mockLogger;
    private readonly Mock<IStatisticsService> _mockStatsService;

    public TopParticipantesChartTests() {
        _mockStatsService = new Mock<IStatisticsService>();
        _mockLogger = new Mock<ILogger<TopParticipantesChart>>();
    }

    [Test]
    public void TopParticipantesChart_Component_CanBeCreated() {
        using var ctx = new TestContext();
        ctx.Services.AddSingleton(_mockStatsService.Object);
        ctx.Services.AddSingleton(_mockLogger.Object);

        IRenderedComponent<TopParticipantesChart> cut = ctx.Render<TopParticipantesChart>();

        cut.Should().NotBeNull();
    }

    [Test]
    public void TopParticipantesChart_ImplementsIDisposable() {
        typeof(TopParticipantesChart).Should().Implement<IDisposable>();
    }

    [Test]
    public void TopParticipantesChart_HasCardStructure() {
        using var ctx = new TestContext();
        ctx.Services.AddSingleton(_mockStatsService.Object);
        ctx.Services.AddSingleton(_mockLogger.Object);

        IRenderedComponent<TopParticipantesChart> cut = ctx.Render<TopParticipantesChart>();

        cut.Find(".card").Should().NotBeNull();
    }

    [Test]
    public void TopParticipantesChart_HasInfoHeader() {
        using var ctx = new TestContext();
        ctx.Services.AddSingleton(_mockStatsService.Object);
        ctx.Services.AddSingleton(_mockLogger.Object);

        IRenderedComponent<TopParticipantesChart> cut = ctx.Render<TopParticipantesChart>();

        cut.Find(".card-header.bg-info").Should().NotBeNull();
    }

    [Test]
    public void TopParticipantesChart_HasModeToggleButtons() {
        using var ctx = new TestContext();
        ctx.Services.AddSingleton(_mockStatsService.Object);
        ctx.Services.AddSingleton(_mockLogger.Object);

        IRenderedComponent<TopParticipantesChart> cut = ctx.Render<TopParticipantesChart>();

        cut.FindAll("button").Should().NotBeEmpty();
    }

    [Test]
    public void TopParticipantesChart_HasRefreshButton() {
        using var ctx = new TestContext();
        ctx.Services.AddSingleton(_mockStatsService.Object);
        ctx.Services.AddSingleton(_mockLogger.Object);

        IRenderedComponent<TopParticipantesChart> cut = ctx.Render<TopParticipantesChart>();

        cut.FindAll("button").Count.Should().BeGreaterThan(1);
    }
}
