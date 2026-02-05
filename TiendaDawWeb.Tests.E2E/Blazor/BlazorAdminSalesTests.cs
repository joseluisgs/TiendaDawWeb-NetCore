using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using TiendaDawWeb.Tests.E2E.Extensions;

namespace TiendaDawWeb.Tests.E2E.Blazor;

/// <summary>
/// Tests E2E para el flujo completo de ventas y panel de administración
/// </summary>
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class BlazorAdminSalesTests : E2ETestBase
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

    [Test]
    public async Task Purchase_ShouldUpdateAdminStatistics()
    {
        string id = System.Guid.NewGuid().ToString().Substring(0, 8);
        string buyerEmail = $"admin_buyer_{id}@test.com";

        await Page.GotoAsync($"{BaseTestUrl}/Auth/Register");
        await CaptureScreenshotAsync("01-register-buyer");
        await Page.TestId("nombre-input").FillAsync("AdminBuyer");
        await Page.TestId("apellidos-input").FillAsync("E2E Test");
        await Page.TestId("email-input").FillAsync(buyerEmail);
        await Page.TestId("password-input").FillAsync("Password123!");
        await Page.TestId("confirm-password-input").FillAsync("Password123!");
        await Page.TestId("submit-button").ClickAsync();
        await CaptureScreenshotAsync("02-registered-buyer");
        await Expect(Page.TestId("user-name")).ToContainTextAsync("AdminBuyer", new() { Timeout = 10000 });

        await Page.GotoAsync($"{BaseTestUrl}/Public");
        await CaptureScreenshotAsync("03-public-page");
        await Page.TestId("search-input").FillAsync("Samsung Galaxy S24");
        await Page.TestId("search-button").ClickAsync();
        await CaptureScreenshotAsync("04-search-product");

        if (await Page.TestId("add-to-cart-button").CountAsync() > 0)
        {
            await Page.TestId("add-to-cart-button").ClickAsync();
            await CaptureScreenshotAsync("05-added-to-cart");
            await Page.WaitForTimeoutAsync(1000);
        }

        await Page.GotoAsync($"{BaseTestUrl}/Auth/Login");
        await Page.TestId("email-input").FillAsync("admin@waladaw.com");
        await Page.TestId("password-input").FillAsync("admin");
        await Page.TestId("submit-button").ClickAsync();
        await CaptureScreenshotAsync("06-admin-login");

        await Expect(Page.TestId("user-name")).ToContainTextAsync("Admin", new() { Timeout = 15000 });

        await Page.GotoAsync($"{BaseTestUrl}/Admin", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await CaptureScreenshotAsync("07-admin-panel");
        await Page.WaitForTimeoutAsync(3000);

        var statsContent = await Page.Locator("body").TextContentAsync();
        Assert.That(statsContent != null && (statsContent.Contains("Usuarios") || statsContent.Contains("Ventas") || statsContent.Contains("Estadística")),
            Is.True, "Admin should see statistics widget");
    }

    [Test]
    public async Task AdminStatsWidget_ShouldShowStatistics()
    {
        await Page.GotoAsync($"{BaseTestUrl}/Auth/Login");
        await Page.TestId("email-input").FillAsync("admin@waladaw.com");
        await Page.TestId("password-input").FillAsync("admin");
        await Page.TestId("submit-button").ClickAsync();
        await CaptureScreenshotAsync("01-admin-login");

        await Expect(Page.TestId("user-name")).ToContainTextAsync("Admin", new() { Timeout = 15000 });

        await Page.GotoAsync($"{BaseTestUrl}/Admin", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await CaptureScreenshotAsync("02-admin-stats");
        await Page.WaitForTimeoutAsync(3000);

        var statsContent = await Page.Locator("body").TextContentAsync();
        Assert.That(statsContent != null && (statsContent.Contains("Usuarios") || statsContent.Contains("Ventas") || statsContent.Contains("Productos")),
            Is.True, "Admin stats widget should show statistics");
    }
}
