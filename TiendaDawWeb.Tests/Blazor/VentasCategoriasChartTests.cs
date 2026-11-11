using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TiendaDawWeb.Shared.Blazor.Admin.Charts;
using TiendaDawWeb.Shared.Services.Stats;
using TestContext = Bunit.BunitContext;

namespace TiendaDawWeb.Tests.Blazor;

public class VentasCategoriasChartTests {
    private readonly Mock<ILogger<VentasCategoriasChart>> _mockLogger;
    private readonly Mock<IStatisticsService> _mockStatsService;

    public VentasCategoriasChartTests() {
        _mockStatsService = new Mock<IStatisticsService>();
        _mockLogger = new Mock<ILogger<VentasCategoriasChart>>();
    }

    [Test]
    public void VentasCategoriasChart_Component_CanBeCreated() {
        using var ctx = new TestContext();
        ctx.Services.AddSingleton(_mockStatsService.Object);
        ctx.Services.AddSingleton(_mockLogger.Object);

        IRenderedComponent<VentasCategoriasChart> cut = ctx.Render<VentasCategoriasChart>();

        cut.Should().NotBeNull();
    }

    [Test]
    public void VentasCategoriasChart_ImplementsIDisposable() {
        typeof(VentasCategoriasChart).Should().Implement<IDisposable>();
    }

    [Test]
    public void VentasCategoriasChart_HasCardStructure() {
        using var ctx = new TestContext();
        ctx.Services.AddSingleton(_mockStatsService.Object);
        ctx.Services.AddSingleton(_mockLogger.Object);

        IRenderedComponent<VentasCategoriasChart> cut = ctx.Render<VentasCategoriasChart>();

        cut.Find(".card").Should().NotBeNull();
    }

    [Test]
    public void VentasCategoriasChart_HasPrimaryHeader() {
        using var ctx = new TestContext();
        ctx.Services.AddSingleton(_mockStatsService.Object);
        ctx.Services.AddSingleton(_mockLogger.Object);

        IRenderedComponent<VentasCategoriasChart> cut = ctx.Render<VentasCategoriasChart>();

        cut.Find(".card-header.bg-primary").Should().NotBeNull();
    }
}
