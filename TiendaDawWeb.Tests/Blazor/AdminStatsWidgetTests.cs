using Bunit;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TiendaDawWeb.Shared.Blazor.Admin;
using TiendaDawWeb.Shared.Errors;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Product;
using TiendaDawWeb.Shared.Services.Purchase;

namespace TiendaDawWeb.Tests.Blazor;

public class AdminStatsWidgetTests
{
    private readonly Mock<IProductService> _mockProductService;
    private readonly Mock<IPurchaseService> _mockPurchaseService;
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly Mock<ILogger<AdminStatsWidget>> _mockLogger;

    public AdminStatsWidgetTests()
    {
        _mockProductService = new Mock<IProductService>();
        _mockPurchaseService = new Mock<IPurchaseService>();
        
        var userStoreMock = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        _mockLogger = new Mock<ILogger<AdminStatsWidget>>();

        var users = new List<User>
        {
            new() { Id = 1, UserName = "user1" },
            new() { Id = 2, UserName = "user2" }
        }.AsQueryable();
        
        _mockUserManager.Setup(u => u.Users)
            .Returns(users);
    }

    [Test]
    public void AdminStatsWidget_Component_CanBeCreated()
    {
#pragma warning disable CS0618
        using var ctx = new Bunit.TestContext();
#pragma warning restore CS0618
        ctx.Services.AddSingleton(_mockProductService.Object);
        ctx.Services.AddSingleton(_mockPurchaseService.Object);
        ctx.Services.AddSingleton(_mockUserManager.Object);
        ctx.Services.AddSingleton(_mockLogger.Object);

        var cut = ctx.Render<AdminStatsWidget>();

        cut.Should().NotBeNull();
    }

    [Test]
    public void AdminStatsWidget_HasCardStructure()
    {
#pragma warning disable CS0618
        using var ctx = new Bunit.TestContext();
#pragma warning restore CS0618
        ctx.Services.AddSingleton(_mockProductService.Object);
        ctx.Services.AddSingleton(_mockPurchaseService.Object);
        ctx.Services.AddSingleton(_mockUserManager.Object);
        ctx.Services.AddSingleton(_mockLogger.Object);

        var cut = ctx.Render<AdminStatsWidget>();

        cut.Find(".card").Should().NotBeNull();
    }

    [Test]
    public void AdminStatsWidget_HasCardHeader()
    {
#pragma warning disable CS0618
        using var ctx = new Bunit.TestContext();
#pragma warning restore CS0618
        ctx.Services.AddSingleton(_mockProductService.Object);
        ctx.Services.AddSingleton(_mockPurchaseService.Object);
        ctx.Services.AddSingleton(_mockUserManager.Object);
        ctx.Services.AddSingleton(_mockLogger.Object);

        var cut = ctx.Render<AdminStatsWidget>();

        cut.Find(".card-header").Should().NotBeNull();
    }

    [Test]
    public void AdminStatsWidget_HasCardBody()
    {
#pragma warning disable CS0618
        using var ctx = new Bunit.TestContext();
#pragma warning restore CS0618
        ctx.Services.AddSingleton(_mockProductService.Object);
        ctx.Services.AddSingleton(_mockPurchaseService.Object);
        ctx.Services.AddSingleton(_mockUserManager.Object);
        ctx.Services.AddSingleton(_mockLogger.Object);

        var cut = ctx.Render<AdminStatsWidget>();

        cut.Find(".card-body").Should().NotBeNull();
    }

    [Test]
    public void AdminStatsWidget_HasRefreshButton()
    {
#pragma warning disable CS0618
        using var ctx = new Bunit.TestContext();
#pragma warning restore CS0618
        ctx.Services.AddSingleton(_mockProductService.Object);
        ctx.Services.AddSingleton(_mockPurchaseService.Object);
        ctx.Services.AddSingleton(_mockUserManager.Object);
        ctx.Services.AddSingleton(_mockLogger.Object);

        var cut = ctx.Render<AdminStatsWidget>();

        cut.Find("button").Should().NotBeNull();
    }

    [Test]
    public void AdminStatsWidget_HasAutoRefreshBadge()
    {
#pragma warning disable CS0618
        using var ctx = new Bunit.TestContext();
#pragma warning restore CS0618
        ctx.Services.AddSingleton(_mockProductService.Object);
        ctx.Services.AddSingleton(_mockPurchaseService.Object);
        ctx.Services.AddSingleton(_mockUserManager.Object);
        ctx.Services.AddSingleton(_mockLogger.Object);

        var cut = ctx.Render<AdminStatsWidget>();

        cut.Find(".badge").Should().NotBeNull();
    }

    [Test]
    public void AdminStatsWidget_HasStatistics()
    {
#pragma warning disable CS0618
        using var ctx = new Bunit.TestContext();
#pragma warning restore CS0618
        ctx.Services.AddSingleton(_mockProductService.Object);
        ctx.Services.AddSingleton(_mockPurchaseService.Object);
        ctx.Services.AddSingleton(_mockUserManager.Object);
        ctx.Services.AddSingleton(_mockLogger.Object);

        var cut = ctx.Render<AdminStatsWidget>();

        cut.FindAll("h3").Should().HaveCount(3);
    }

    [Test]
    public void AdminStatsWidget_DisplaysLastUpdate()
    {
#pragma warning disable CS0618
        using var ctx = new Bunit.TestContext();
#pragma warning restore CS0618
        ctx.Services.AddSingleton(_mockProductService.Object);
        ctx.Services.AddSingleton(_mockPurchaseService.Object);
        ctx.Services.AddSingleton(_mockUserManager.Object);
        ctx.Services.AddSingleton(_mockLogger.Object);

        var cut = ctx.Render<AdminStatsWidget>();

        cut.Find(".text-end").Should().NotBeNull();
    }

    [Test]
    public void AdminStatsWidget_ImplementsIDisposable()
    {
        typeof(AdminStatsWidget).Should().Implement<IDisposable>();
    }

    [Test]
    public void AdminStatsWidget_HasRowLayout()
    {
#pragma warning disable CS0618
        using var ctx = new Bunit.TestContext();
#pragma warning restore CS0618
        ctx.Services.AddSingleton(_mockProductService.Object);
        ctx.Services.AddSingleton(_mockPurchaseService.Object);
        ctx.Services.AddSingleton(_mockUserManager.Object);
        ctx.Services.AddSingleton(_mockLogger.Object);

        var cut = ctx.Render<AdminStatsWidget>();

        cut.Find(".row").Should().NotBeNull();
    }

    [Test]
    public void AdminStatsWidget_HasThreeColumns()
    {
#pragma warning disable CS0618
        using var ctx = new Bunit.TestContext();
#pragma warning restore CS0618
        ctx.Services.AddSingleton(_mockProductService.Object);
        ctx.Services.AddSingleton(_mockPurchaseService.Object);
        ctx.Services.AddSingleton(_mockUserManager.Object);
        ctx.Services.AddSingleton(_mockLogger.Object);

        var cut = ctx.Render<AdminStatsWidget>();

        cut.FindAll(".col-md-4").Should().HaveCount(3);
    }
}
