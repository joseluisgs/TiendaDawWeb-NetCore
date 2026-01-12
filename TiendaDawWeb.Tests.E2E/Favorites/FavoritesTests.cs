using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using TiendaDawWeb.Tests.E2E.Extensions;

namespace TiendaDawWeb.Tests.E2E.Favorites;

/**
 * MÓDULO DE FAVORITOS (E2E - AJAX)
 * 
 * OBJETIVO: Probar la interactividad asíncrona sin recarga de página y persistencia.
 * TECNOLOGÍAS TESTEADAS: Fetch API, JavaScript (favorites.js), API Controllers, AJAX Persistencia.
 */
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class FavoritesTests : E2ETestBase
{
    private const string BaseUrl = "http://localhost:5000";

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
    public async Task ToggleFavorite_ShouldWorkWithNewUser()
    {
        string id = System.Guid.NewGuid().ToString().Substring(0, 8);
        await Page.GotoAsync($"{BaseUrl}/Auth/Register");
        await CaptureScreenshotAsync("01-register-page");

        await Page.TestId("nombre-input").FillAsync("FavUser");
        await Page.TestId("apellidos-input").FillAsync("Test");
        await Page.TestId("email-input").FillAsync($"fav_{id}@test.com");
        await Page.TestId("password-input").FillAsync("Password123!");
        await Page.TestId("confirm-password-input").FillAsync("Password123!");
        await Page.TestId("submit-button").ClickAsync();
        await CaptureScreenshotAsync("02-after-login");

        await Expect(Page.TestId("user-name")).ToContainTextAsync("FavUser");

        await Page.GotoAsync($"{BaseUrl}/Public");
        await CaptureScreenshotAsync("03-product-list");
        
        var products = Page.TestId("product-card");
        await Expect(products.First).ToBeVisibleAsync();

        await products.Nth(1).Locator("a").First.ClickAsync();
        await CaptureScreenshotAsync("04-product-details");

        var favoriteBtn = Page.TestId("favorite-button");
        await Expect(favoriteBtn).ToBeVisibleAsync();
        await favoriteBtn.ClickAsync();
        await CaptureScreenshotAsync("05-after-favorite");

        await Expect(Page.Locator(".toast-body")).ToBeVisibleAsync();
        await Expect(favoriteBtn).ToHaveClassAsync(new System.Text.RegularExpressions.Regex(".*btn-danger.*"));

        await Page.ReloadAsync();
        await CaptureScreenshotAsync("06-after-reload");
        await Expect(favoriteBtn).ToHaveClassAsync(new System.Text.RegularExpressions.Regex(".*btn-danger.*"));
    }
}