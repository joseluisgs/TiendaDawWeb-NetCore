using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace TiendaDawWeb.Tests.E2E;

/**
 * MÓDULO DE LOCALIZACIÓN (E2E - i18n)
 * 
 * OBJETIVO: Validar que el cambio de idioma en el Navbar actualiza las etiquetas de la UI.
 * TECNOLOGÍAS TESTEADAS: ASP.NET Core Localization, Cookies de Cultura, Playwright Context Options.
 */
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class LocalizationTests : PageTest
{
    private const string BaseUrl = "http://localhost:5000";

    /// <summary>
    /// Forzamos el inicio en Español para tener un estado inicial conocido.
    /// </summary>
    public override BrowserNewContextOptions ContextOptions()
    {
        return new BrowserNewContextOptions
        {
            Locale = "es-ES",
            TimezoneId = "Europe/Madrid"
        };
    }

    /// <summary>
    /// Test: Cambio de Idioma. Verifica que al cambiar de ES a EN, las etiquetas se actualizan.
    /// </summary>
    [Test]
    public async Task ChangeLanguage_ShouldSwitchBetweenEsAndEn()
    {
        // 1. Acción: Ir a la página pública
        await Page.GotoAsync($"{BaseUrl}/Public");

        // 2. Verificación Inicial (Español)
        // Buscamos el label de búsqueda que en ES debe ser "Buscar"
        var searchLabel = Page.Locator("label.form-label").First;
        await Expect(searchLabel).ToContainTextAsync("Buscar");

        // 3. Acción: Cambiar a Inglés mediante el Navbar
        // El selector del dropdown de idioma usa el icono del globo
        await Page.Locator(".nav-link.dropdown-toggle:has(.bi-globe)").ClickAsync();
        // Hacemos click en el enlace de English
        await Page.Locator("a.dropdown-item:has-text('English')").ClickAsync();

        // 4. Verificación Final (Inglés)
        // El label debe haber cambiado a "Search" automáticamente
        await Expect(searchLabel).ToHaveTextAsync("Search");
        
        // Verificamos también el placeholder del input de búsqueda (usando el de main para evitar ambigüedad)
        var searchInput = Page.Locator("main input[name='q']");
        await Expect(searchInput).ToHaveAttributeAsync("placeholder", "Search products...");
    }
}