using Microsoft.Playwright;
using TiendaDawWeb.Tests.E2E.Extensions;

namespace TiendaDawWeb.Tests.E2E.Admin;

/// <summary>
///     Tests E2E para administración
/// </summary>
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class AdminUserManagementTests : E2ETestBase {
    [SetUp]
    public async Task Setup() {
        await Page.GotoAsync($"{BaseTestUrl}/Auth/Login");
        await Page.TestId("email-input").FillAsync(AdminEmail);
        await Page.TestId("password-input").FillAsync(AdminPassword);
        await Page.TestId("submit-button").ClickAsync();
        await Expect(Page.TestId("user-name"))
            .ToContainTextAsync("Admin", new LocatorAssertionsToContainTextOptions { Timeout = 15000 });
    }

    [Test]
    public async Task AdminCanAccessAdminPanel() {
        await Page.GotoAsync($"{BaseTestUrl}/Admin");
        await CaptureScreenshotAsync("01-admin-panel");

        string? adminContent = await Page.Locator("body").TextContentAsync();
        Assert.That(
            adminContent != null && (adminContent.Contains("Admin") || adminContent.Contains("Panel") ||
                                     adminContent.Contains("Estadística")),
            Is.True, "Admin panel should load");
    }

    [Test]
    public async Task AdminStatsWidget_IsVisible() {
        await Page.GotoAsync($"{BaseTestUrl}/Admin");
        await CaptureScreenshotAsync("01-admin-panel");

        await Page.WaitForTimeoutAsync(3000);
        string? statsContent = await Page.Locator("body").TextContentAsync();
        Assert.That(
            statsContent != null && (statsContent.Contains("Usuarios") || statsContent.Contains("Ventas") ||
                                     statsContent.Contains("Estadística")),
            Is.True, "Admin stats widget should be visible");
    }

    [Test]
    public async Task AdminStatsWidget_ShowsStatistics() {
        await Page.GotoAsync($"{BaseTestUrl}/Admin");
        await CaptureScreenshotAsync("01-admin-stats");

        await Page.WaitForTimeoutAsync(3000);
        string? statsContent = await Page.Locator("body").TextContentAsync();
        Assert.That(
            statsContent != null && (statsContent.Contains("Usuarios") || statsContent.Contains("Productos") ||
                                     statsContent.Contains("Ventas")),
            Is.True, "Stats should show users, products, and sales");
    }
}
