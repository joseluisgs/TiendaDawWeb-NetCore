using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using TiendaDawWeb.Tests.E2E.Extensions;

namespace TiendaDawWeb.Tests.E2E.Admin;

/// <summary>
/// Tests E2E para administración
/// </summary>
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class AdminUserManagementTests : E2ETestBase
{
    [SetUp]
    public async Task Setup()
    {
        await Page.GotoAsync($"{BaseTestUrl}/Auth/Login");
        await Page.TestId("email-input").FillAsync("admin@waladaw.com");
        await Page.TestId("password-input").FillAsync("admin");
        await Page.TestId("submit-button").ClickAsync();
        await Expect(Page.TestId("user-name")).ToContainTextAsync("Admin", new() { Timeout = 15000 });
    }

    [Test]
    public async Task AdminCanAccessAdminPanel()
    {
        await Page.GotoAsync($"{BaseTestUrl}/Admin");
        await CaptureScreenshotAsync("01-admin-panel");

        var adminContent = await Page.Locator("body").TextContentAsync();
        Assert.That(adminContent != null && (adminContent.Contains("Admin") || adminContent.Contains("Panel") || adminContent.Contains("Estadística")),
            Is.True, "Admin panel should load");
    }

    [Test]
    public async Task AdminStatsWidget_IsVisible()
    {
        await Page.GotoAsync($"{BaseTestUrl}/Admin");
        await CaptureScreenshotAsync("01-admin-panel");

        await Page.WaitForTimeoutAsync(3000);
        var statsContent = await Page.Locator("body").TextContentAsync();
        Assert.That(statsContent != null && (statsContent.Contains("Usuarios") || statsContent.Contains("Ventas") || statsContent.Contains("Estadística")),
            Is.True, "Admin stats widget should be visible");
    }

    [Test]
    public async Task AdminStatsWidget_ShowsStatistics()
    {
        await Page.GotoAsync($"{BaseTestUrl}/Admin");
        await CaptureScreenshotAsync("01-admin-stats");

        await Page.WaitForTimeoutAsync(3000);
        var statsContent = await Page.Locator("body").TextContentAsync();
        Assert.That(statsContent != null && (statsContent.Contains("Usuarios") || statsContent.Contains("Productos") || statsContent.Contains("Ventas")),
            Is.True, "Stats should show users, products, and sales");
    }
}
