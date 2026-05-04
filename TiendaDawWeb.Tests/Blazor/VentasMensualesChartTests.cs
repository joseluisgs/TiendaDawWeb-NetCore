using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TiendaDawWeb.Shared.Blazor.Admin.Charts;
using TiendaDawWeb.Shared.Services.Stats;

namespace TiendaDawWeb.Tests.Blazor;

public class VentasMensualesChartTests
{
    private readonly Mock<IStatisticsService> _mockStatsService;
    private readonly Mock<ILogger<VentasMensualesChart>> _mockLogger;

    public VentasMensualesChartTests()
    {
        _mockStatsService = new Mock<IStatisticsService>();
        _mockLogger = new Mock<ILogger<VentasMensualesChart>>();
    }

    [Test]
    public void VentasMensualesChart_Component_CanBeCreated()
    {
#pragma warning disable CS0618
        using var ctx = new Bunit.TestContext();
#pragma warning restore CS0618
        ctx.Services.AddSingleton(_mockStatsService.Object);
        ctx.Services.AddSingleton(_mockLogger.Object);

        var cut = ctx.Render<VentasMensualesChart>();

        cut.Should().NotBeNull();
    }

    [Test]
    public void VentasMensualesChart_ImplementsIDisposable()
    {
        typeof(VentasMensualesChart).Should().Implement<IDisposable>();
    }

    [Test]
    public void VentasMensualesChart_HasCardStructure()
    {
#pragma warning disable CS0618
        using var ctx = new Bunit.TestContext();
#pragma warning restore CS0618
        ctx.Services.AddSingleton(_mockStatsService.Object);
        ctx.Services.AddSingleton(_mockLogger.Object);

        var cut = ctx.Render<VentasMensualesChart>();

        cut.Find(".card").Should().NotBeNull();
    }

    [Test]
    public void VentasMensualesChart_HasWarningHeader()
    {
#pragma warning disable CS0618
        using var ctx = new Bunit.TestContext();
#pragma warning restore CS0618
        ctx.Services.AddSingleton(_mockStatsService.Object);
        ctx.Services.AddSingleton(_mockLogger.Object);

        var cut = ctx.Render<VentasMensualesChart>();

        cut.Find(".card-header.bg-warning").Should().NotBeNull();
    }

    [Test]
    public void VentasMensualesChart_HasRefreshButton()
    {
#pragma warning disable CS0618
        using var ctx = new Bunit.TestContext();
#pragma warning restore CS0618
        ctx.Services.AddSingleton(_mockStatsService.Object);
        ctx.Services.AddSingleton(_mockLogger.Object);

        var cut = ctx.Render<VentasMensualesChart>();

        cut.Find("button").Should().NotBeNull();
    }

    [Test]
    public void VentasMensualesChart_HasAutoRefreshBadge()
    {
#pragma warning disable CS0618
        using var ctx = new Bunit.TestContext();
#pragma warning restore CS0618
        ctx.Services.AddSingleton(_mockStatsService.Object);
        ctx.Services.AddSingleton(_mockLogger.Object);

        var cut = ctx.Render<VentasMensualesChart>();

        cut.Find(".badge").Should().NotBeNull();
    }
}