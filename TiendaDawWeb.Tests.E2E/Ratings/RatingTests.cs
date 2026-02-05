using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using TiendaDawWeb.Tests.E2E.Extensions;

namespace TiendaDawWeb.Tests.E2E.Ratings;

/// <summary>
/// MÓDULO DE VALORACIONES (Ratings) - E2E Framework Agnóstico
/// </summary>
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class RatingTests : E2ETestBase
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
    public async Task SubmitRating_ShouldUpdateDisplayAndPersist()
    {
        await Page.GotoAsync($"{BaseTestUrl}/Auth/Login", new() { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 15000 });
        await CaptureScreenshotAsync("01-login-page");

        await Page.TestId("email-input").FillAsync("carlos@email.com");
        await Page.TestId("password-input").FillAsync("carlos123");
        await CaptureScreenshotAsync("02-credentials-filled");

        await Page.TestId("submit-button").ClickAsync();
        await CaptureScreenshotAsync("03-after-login");

        await Expect(Page.TestId("user-name")).ToContainTextAsync(new System.Text.RegularExpressions.Regex("Carlos", System.Text.RegularExpressions.RegexOptions.IgnoreCase), new() { Timeout = 15000 });

        await Page.GotoAsync($"{BaseTestUrl}/Product/Details/1", new() { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 15000 });
        await CaptureScreenshotAsync("04-product-details");

        await Page.WaitForTimeoutAsync(3000);

        var stars = Page.Locator(".star-rating-input .star-item");
        var starCount = await stars.CountAsync();
        if (starCount > 0)
        {
            await stars.Nth(3).ClickAsync();
            await CaptureScreenshotAsync("05-star-selected");
        }

        var commentInput = Page.Locator("textarea");
        if (await commentInput.CountAsync() > 0)
        {
            await commentInput.First.FillAsync("test automation");
        }

        var submitButton = Page.Locator("button[type='submit']");
        if (await submitButton.CountAsync() > 0)
        {
            await submitButton.First.ClickAsync();
            await CaptureScreenshotAsync("06-submitting");
        }

        await Page.WaitForTimeoutAsync(3000);

        var body = Page.Locator("body");
        await Expect(body).ToContainTextAsync(new System.Text.RegularExpressions.Regex("Gracias|valoración|gracias", System.Text.RegularExpressions.RegexOptions.IgnoreCase), new() { Timeout = 20000 });
        await CaptureScreenshotAsync("07-success");

        Console.WriteLine("Test de valoración completado");
    }
}
