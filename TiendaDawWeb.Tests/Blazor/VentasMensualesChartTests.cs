using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TiendaDawWeb.Shared.Blazor.Admin.Charts;
using TiendaDawWeb.Shared.Services.Stats;
using TestContext = Bunit.BunitContext;

namespace TiendaDawWeb.Tests.Blazor;

public class VentasMensualesChartTests {
    private readonly Mock<ILogger<VentasMensualesChart>> _mockLogger;
    private readonly Mock<IStatisticsService> _mockStatsService;

    public VentasMensualesChartTests() {
        _mockStatsService = new Mock<IStatisticsService>();
        _mockLogger = new Mock<ILogger<VentasMensualesChart>>();
    }

    [Test]
    public void VentasMensualesChart_Component_CanBeCreated() {
        using var ctx = new TestContext();
        ctx.Services.AddSingleton(_mockStatsService.Object);
        ctx.Services.AddSingleton(_mockLogger.Object);

        IRenderedComponent<VentasMensualesChart> cut = ctx.Render<VentasMensualesChart>();

        cut.Should().NotBeNull();
    }

    [Test]
    public void VentasMensualesChart_ImplementsIDisposable() {
        typeof(VentasMensualesChart).Should().Implement<IDisposable>();
    }

    [Test]
    public void VentasMensualesChart_HasCardStructure() {
        using var ctx = new TestContext();
        ctx.Services.AddSingleton(_mockStatsService.Object);
        ctx.Services.AddSingleton(_mockLogger.Object);

        IRenderedComponent<VentasMensualesChart> cut = ctx.Render<VentasMensualesChart>();

        cut.Find(".card").Should().NotBeNull();
    }

    [Test]
    public void VentasMensualesChart_HasWarningHeader() {
        using var ctx = new TestContext();
        ctx.Services.AddSingleton(_mockStatsService.Object);
        ctx.Services.AddSingleton(_mockLogger.Object);

        IRenderedComponent<VentasMensualesChart> cut = ctx.Render<VentasMensualesChart>();

        cut.Find(".card-header.bg-warning").Should().NotBeNull();
    }

    [Test]
    public void VentasMensualesChart_HasRefreshButton() {
        using var ctx = new TestContext();
        ctx.Services.AddSingleton(_mockStatsService.Object);
        ctx.Services.AddSingleton(_mockLogger.Object);

        IRenderedComponent<VentasMensualesChart> cut = ctx.Render<VentasMensualesChart>();

        cut.Find("button").Should().NotBeNull();
    }

    [Test]
    public void VentasMensualesChart_HasAutoRefreshBadge() {
        using var ctx = new TestContext();
        ctx.Services.AddSingleton(_mockStatsService.Object);
        ctx.Services.AddSingleton(_mockLogger.Object);

        IRenderedComponent<VentasMensualesChart> cut = ctx.Render<VentasMensualesChart>();

        cut.Find(".badge").Should().NotBeNull();
    }
}
