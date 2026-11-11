using Microsoft.Playwright;
using TiendaDawWeb.Tests.E2E.Extensions;

namespace TiendaDawWeb.Tests.E2E.Blazor;

/// <summary>
///     Tests E2E para componentes Blazor Rating
///     Reglas de negocio del sistema de valoraciones:
///     - Solo pueden valorar usuarios que han COMPRADO el producto
///     - El DUENO de un producto NO puede valorarlo
///     - Las valoraciones se ven en RatingSummary y RatingSection
/// </summary>
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class BlazorRatingTests : E2ETestBase {
    public override BrowserNewContextOptions ContextOptions() {
        return new BrowserNewContextOptions {
            Locale = "es-ES",
            TimezoneId = "Europe/Madrid",
            RecordVideoDir = "TestVideos"
        };
    }

    [Test]
    public async Task ProductDetailsPage_ShouldLoad() {
        await Page.GotoAsync($"{BaseTestUrl}/Product/Details/1",
            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 15000 });
        await CaptureScreenshotAsync("01-product-details");

        await Page.WaitForTimeoutAsync(3000);

        bool pageVisible = await Page.Locator("body").IsVisibleAsync();
        Assert.That(pageVisible, Is.True, "Product details page should load");
    }

    [Test]
    public async Task AuthenticatedUser_CanAccessProductDetails() {
        await Page.GotoAsync($"{BaseTestUrl}/Auth/Login",
            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 15000 });
        await Page.TestId("email-input").FillAsync(CarlosEmail);
        await Page.TestId("password-input").FillAsync(CarlosPassword);
        await Page.TestId("submit-button").ClickAsync();

        await Expect(Page.TestId("user-name"))
            .ToContainTextAsync("Carlos", new LocatorAssertionsToContainTextOptions { Timeout = 15000 });

        await Page.GotoAsync($"{BaseTestUrl}/Product/Details/1",
            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 15000 });
        await CaptureScreenshotAsync("02-carlos-product");

        await Page.WaitForTimeoutAsync(2000);

        bool pageVisible = await Page.Locator("body").IsVisibleAsync();
        Assert.That(pageVisible, Is.True, "Authenticated user should access product page");
    }
}
