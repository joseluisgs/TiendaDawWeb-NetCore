using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using TiendaDawWeb.Tests.E2E.Extensions;

namespace TiendaDawWeb.Tests.E2E.Ratings;

/// <summary>
/// MÓDULO DE VALORACIONES (Ratings) - E2E
///
/// OBJETIVO: Probar el flujo completo de valoración de productos con Blazor Server.
/// TECNOLOGÍAS TESTEADAS: Blazor Server Components, SignalR, RatingService, StateContainer.
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
        try
        {
            await Page.GotoAsync($"{BaseTestUrl}/Auth/Login", new() { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 15000 });
            await CaptureScreenshotAsync("01-login-page");
            
            await Page.TestId("email-input").FillAsync("carlos@email.com");
            await Page.TestId("password-input").FillAsync("carlos123");
            await CaptureScreenshotAsync("02-credentials-filled");
            
            await Page.TestId("submit-button").ClickAsync();

            await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*"), new() { Timeout = 10000 });
            await Expect(Page.TestId("user-name")).ToContainTextAsync(new System.Text.RegularExpressions.Regex("Carlos", System.Text.RegularExpressions.RegexOptions.IgnoreCase), new() { Timeout = 10000 });

            await Page.GotoAsync($"{BaseTestUrl}/Product/Details/1", new() { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 15000 });
            await CaptureScreenshotAsync("03-product-details");

            await Expect(Page.TestId("product-title")).ToBeVisibleAsync(new() { Timeout = 10000 });

            // Esperar a que el componente Blazor de rating cargue completamente
            await Page.WaitForSelectorAsync("#ratingsList", new() { Timeout = 10000 });

            var ratingForm = Page.Locator("#cardFormulario");
            await Expect(ratingForm).ToBeVisibleAsync(new() { Timeout = 15000 });

            var stars = Page.Locator(".star-rating-input .star-item");
            await Expect(stars.Nth(0)).ToBeVisibleAsync(new() { Timeout = 10000 });

            await stars.Nth(3).ClickAsync();
            await CaptureScreenshotAsync("04-star-selected");

            await Expect(stars.Nth(3)).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-warning"), new() { Timeout = 5000 });

            var commentInput = Page.Locator("textarea[placeholder*='Qué te ha parecido']");
            await Expect(commentInput).ToBeVisibleAsync(new() { Timeout = 5000 });
            await commentInput.FillAsync("test");
            await CaptureScreenshotAsync("05-comment-filled");

            var submitButton = Page.Locator("button[type='submit']:has-text('Enviar Valoración')");
            await Expect(submitButton).ToBeVisibleAsync(new() { Timeout = 5000 });
            await submitButton.ClickAsync();
            await CaptureScreenshotAsync("06-submitting");

            await Task.Delay(2000);

            var thanksMessage = Page.Locator("text=/Gracias por tu valoración/i");
            await Expect(thanksMessage).ToBeVisibleAsync(new() { Timeout = 15000 });
            await CaptureScreenshotAsync("07-thanks-message");

            var userRatingSection = thanksMessage.Locator("..").Locator("..");
            await Expect(userRatingSection).ToContainTextAsync("4 / 5", new() { Timeout = 5000 });
            await Expect(userRatingSection).ToContainTextAsync("test", new() { Timeout = 5000 });

            var ratingsList = Page.Locator("#ratingsList");
            await Expect(ratingsList).ToContainTextAsync("Carlos", new() { Timeout = 5000 });
            await Expect(ratingsList).ToContainTextAsync("test", new() { Timeout = 5000 });
            await CaptureScreenshotAsync("08-rating-added");

            await Page.ReloadAsync(new() { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 15000 });
            await CaptureScreenshotAsync("09-after-reload");

            await Expect(Page.TestId("product-title")).ToBeVisibleAsync(new() { Timeout = 10000 });

            var thanksMessageReload = Page.Locator("text=/Gracias por tu valoración/i");
            await Expect(thanksMessageReload).ToBeVisibleAsync(new() { Timeout = 10000 });

            var userRatingSectionReload = thanksMessageReload.Locator("..").Locator("..");
            await Expect(userRatingSectionReload).ToContainTextAsync("test", new() { Timeout = 5000 });
            await Expect(userRatingSectionReload).ToContainTextAsync("4 / 5", new() { Timeout = 5000 });
            await CaptureScreenshotAsync("10-persistence-verified");

            Console.WriteLine("Test de valoración completado exitosamente");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR en SubmitRating: {ex.Message}");
            await Page.ScreenshotAsync(new() { Path = "rating-test-failure.png" });
            throw;
        }
    }
}
