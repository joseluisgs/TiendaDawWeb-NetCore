using Microsoft.Playwright;
using TiendaDawWeb.Tests.E2E.Extensions;

namespace TiendaDawWeb.Tests.E2E.Blazor;

/// <summary>
///     Tests E2E para componentes Blazor - AdminStatsWidget
/// </summary>
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class BlazorAdminTests : E2ETestBase {
    [SetUp]
    public async Task Setup() {
        await Page.GotoAsync($"{BaseTestUrl}/Auth/Login",
            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await Page.TestId("email-input").FillAsync(AdminEmail);
        await Page.TestId("password-input").FillAsync(AdminPassword);
        await Page.TestId("submit-button").ClickAsync();
        await Expect(Page.TestId("user-name"))
            .ToContainTextAsync("Admin", new LocatorAssertionsToContainTextOptions { Timeout = 15000 });
    }

    public override BrowserNewContextOptions ContextOptions() {
        return new BrowserNewContextOptions {
            Locale = "es-ES",
            TimezoneId = "Europe/Madrid",
            RecordVideoDir = "TestVideos"
        };
    }

    [Test]
    public async Task AdminStatsWidget_ShouldLoad_WhenVisitingAdminPage() {
        await Page.GotoAsync($"{BaseTestUrl}/Admin",
            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 15000 });
        await CaptureScreenshotAsync("01-admin-page");

        await Page.WaitForTimeoutAsync(3000);

        ILocator body = Page.Locator("body");
        string? content = await body.TextContentAsync();

        Assert.That(
            content != null && (content.Contains("Estadística") || content.Contains("estadística") ||
                                content.Contains("stats") || content.Contains("dashboard") ||
                                content.Contains("Total")),
            Is.True, "AdminStatsWidget should be visible");
    }

    [Test]
    public async Task AdminStatsWidget_ShouldShowStatistics_WhenAdminIsLoggedIn() {
        await Page.GotoAsync($"{BaseTestUrl}/Admin",
            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 15000 });
        await CaptureScreenshotAsync("02-admin-stats");

        await Page.WaitForTimeoutAsync(3000);

        ILocator body = Page.Locator("body");
        string? content = await body.TextContentAsync();

        Assert.That(
            content != null && (content.Contains("producto") || content.Contains("usuario") ||
                                content.Contains("venta") || content.Contains("€") || content.Contains("Total")),
            Is.True, "AdminStatsWidget should show statistics");
    }

    [Test]
    public async Task AdminPage_ShouldContainWidgets_WhenAdminIsLoggedIn() {
        await Page.GotoAsync($"{BaseTestUrl}/Admin",
            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 15000 });
        await CaptureScreenshotAsync("03-admin-widgets");

        await Page.WaitForTimeoutAsync(3000);

        ILocator widgets = Page.Locator(".card, .widget, [class*='stat'], [class*='widget']");
        Assert.That(await widgets.CountAsync(), Is.GreaterThan(0), "Admin page should have widgets");
    }
}
