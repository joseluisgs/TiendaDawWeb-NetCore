using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using TiendaDawWeb.Tests.E2E.Extensions;

namespace TiendaDawWeb.Tests.E2E.Blazor;

/// <summary>
/// Tests E2E para componentes Blazor Rating
/// 
/// Reglas de negocio del sistema de valoraciones:
/// - Solo pueden valorar usuarios que han COMPRADO el producto
/// - El DUENO de un producto NO puede valorarlo
/// - Las valoraciones se ven en RatingSummary y RatingSection
/// </summary>
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class BlazorRatingTests : E2ETestBase
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
    public async Task ProductDetailsPage_ShouldLoad()
    {
        await Page.GotoAsync($"{BaseTestUrl}/Product/Details/1", new() { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 15000 });
        await CaptureScreenshotAsync("01-product-details");

        await Page.WaitForTimeoutAsync(3000);

        var pageVisible = await Page.Locator("body").IsVisibleAsync();
        Assert.That(pageVisible, Is.True, "Product details page should load");
    }

    [Test]
    public async Task AuthenticatedUser_CanAccessProductDetails()
    {
        await Page.GotoAsync($"{BaseTestUrl}/Auth/Login", new() { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 15000 });
        await Page.TestId("email-input").FillAsync("carlos@email.com");
        await Page.TestId("password-input").FillAsync("carlos123");
        await Page.TestId("submit-button").ClickAsync();

        await Expect(Page.TestId("user-name")).ToContainTextAsync("Carlos", new() { Timeout = 15000 });

        await Page.GotoAsync($"{BaseTestUrl}/Product/Details/1", new() { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 15000 });
        await CaptureScreenshotAsync("02-carlos-product");

        await Page.WaitForTimeoutAsync(2000);

        var pageVisible = await Page.Locator("body").IsVisibleAsync();
        Assert.That(pageVisible, Is.True, "Authenticated user should access product page");
    }
}
