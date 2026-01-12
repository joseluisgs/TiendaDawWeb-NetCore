using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using TiendaDawWeb.Tests.E2E.Extensions;

namespace TiendaDawWeb.Tests.E2E.Localization;

/**
 * MÓDULO DE LOCALIZACIÓN (E2E - i18n)
 * 
 * OBJETIVO: Validar que el cambio de idioma en el Navbar actualiza las etiquetas de la UI.
 * TECNOLOGÍAS TESTEADAS: ASP.NET Core Localization, Cookies de Cultura, Playwright Context Options.
 */
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class LocalizationTests : E2ETestBase
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
    public async Task ChangeLanguage_ShouldSwitchBetweenEsAndEn()
    {
        await Page.GotoAsync($"{BaseUrl}/Public");
        await CaptureScreenshotAsync("01-public-spanish");

        var searchLabel = Page.Locator("label.form-label").First;
        await Expect(searchLabel).ToContainTextAsync("Buscar");

        await Page.Locator(".nav-link.dropdown-toggle:has(.bi-globe)").ClickAsync();
        await CaptureScreenshotAsync("02-language-dropdown");
        
        await Page.Locator("a.dropdown-item:has-text('English')").ClickAsync();
        await CaptureScreenshotAsync("03-public-english");

        await Expect(searchLabel).ToHaveTextAsync("Search");
        
        var searchInput = Page.TestId("search-input");
        await Expect(searchInput).ToHaveAttributeAsync("placeholder", "Search products...");
    }
}