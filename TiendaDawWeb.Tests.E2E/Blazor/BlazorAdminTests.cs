using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using TiendaDawWeb.Tests.E2E.Extensions;

namespace TiendaDawWeb.Tests.E2E.Blazor;

/// <summary>
/// Tests E2E para componentes Blazor - AdminStatsWidget
/// </summary>
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class BlazorAdminTests : E2ETestBase
{
    public override BrowserNewContextOptions ContextOptions()
    {
        return new BrowserNewContextOptions
        {
            Locale = "es-ES",
            TimezoneId = "Europe/Madrid",
            RecordVideoDir = "TestVideos"
        };
    }

    [SetUp]
    public async Task Setup()
    {
        await Page.GotoAsync($"{BaseTestUrl}/Auth/Login", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await Page.TestId("email-input").FillAsync("admin@waladaw.com");
        await Page.TestId("password-input").FillAsync("admin");
        await Page.TestId("submit-button").ClickAsync();
        await Expect(Page.TestId("user-name")).ToContainTextAsync("Admin", new() { Timeout = 15000 });
    }

    [Test]
    public async Task AdminStatsWidget_ShouldLoad_WhenVisitingAdminPage()
    {
        await Page.GotoAsync($"{BaseTestUrl}/Admin", new() { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 15000 });
        await CaptureScreenshotAsync("01-admin-page");

        await Page.WaitForTimeoutAsync(3000);

        var body = Page.Locator("body");
        var content = await body.TextContentAsync();

        Assert.That(content != null && (content.Contains("Estadística") || content.Contains("estadística") || content.Contains("stats") || content.Contains("dashboard") || content.Contains("Total")),
            Is.True, "AdminStatsWidget should be visible");
    }

    [Test]
    public async Task AdminStatsWidget_ShouldShowStatistics_WhenAdminIsLoggedIn()
    {
        await Page.GotoAsync($"{BaseTestUrl}/Admin", new() { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 15000 });
        await CaptureScreenshotAsync("02-admin-stats");

        await Page.WaitForTimeoutAsync(3000);

        var body = Page.Locator("body");
        var content = await body.TextContentAsync();

        Assert.That(content != null && (content.Contains("producto") || content.Contains("usuario") || content.Contains("venta") || content.Contains("€") || content.Contains("Total")),
            Is.True, "AdminStatsWidget should show statistics");
    }

    [Test]
    public async Task AdminPage_ShouldContainWidgets_WhenAdminIsLoggedIn()
    {
        await Page.GotoAsync($"{BaseTestUrl}/Admin", new() { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 15000 });
        await CaptureScreenshotAsync("03-admin-widgets");

        await Page.WaitForTimeoutAsync(3000);

        var widgets = Page.Locator(".card, .widget, [class*='stat'], [class*='widget']");
        Assert.That(await widgets.CountAsync(), Is.GreaterThan(0), "Admin page should have widgets");
    }
}
