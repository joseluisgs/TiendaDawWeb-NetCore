using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace TiendaDawWeb.Tests.E2E;

/**
 * MÓDULO DE FAVORITOS (E2E - AJAX)
 * 
 * OBJETIVO: Probar la interactividad asíncrona sin recarga de página y persistencia.
 * TECNOLOGÍAS TESTEADAS: Fetch API, JavaScript (favorites.js), API Controllers, AJAX Persistencia.
 */
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class FavoritesTests : PageTest
{
    private const string BaseUrl = "http://localhost:5000";

    /// <summary>
    /// Configuración del contexto: Locale es-ES.
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
    /// Test: Alternar favoritos mediante AJAX. Debe cambiar el estado visual y persistir.
    /// </summary>
    [Test]
    public async Task ToggleFavorite_ShouldWorkWithNewUser()
    {
        // 1. Registro de usuario nuevo para asegurar estado limpio e independencia de propiedad
        string id = System.Guid.NewGuid().ToString().Substring(0, 8);
        await Page.GotoAsync($"{BaseUrl}/Auth/Register");
        await Page.FillAsync("#Nombre", "FavUser");
        await Page.FillAsync("#Apellidos", "Test");
        await Page.FillAsync("#Email", $"fav_{id}@test.com");
        await Page.FillAsync("#Password", "Password123!");
        await Page.FillAsync("#ConfirmPassword", "Password123!");
        await Page.Locator(".card-body form button[type='submit']").ClickAsync();

        await Expect(Page.Locator(".navbar")).ToContainTextAsync("FavUser");

        // 2. Acción: Ir al listado público y entrar al segundo producto (evita el primero que podría ser del usuario)
        await Page.GotoAsync($"{BaseUrl}/Public");
        var products = Page.Locator(".producto-card");
        await Expect(products.First).ToBeVisibleAsync();

        // Click en el segundo producto para asegurar que NO es del usuario nuevo
        await products.Nth(1).Locator("a").First.ClickAsync();

        // 3. Acción: Click en botón de favoritos (AJAX)
        var favoriteBtn = Page.Locator(".favorite-btn");
        await Expect(favoriteBtn).ToBeVisibleAsync();
        await favoriteBtn.ClickAsync();

        // 4. Verificación: El script debe lanzar el Toast y cambiar la clase a relleno (btn-danger)
        await Expect(Page.Locator(".toast-body")).ToBeVisibleAsync();
        await Expect(favoriteBtn).ToHaveClassAsync(new System.Text.RegularExpressions.Regex(".*btn-danger.*"));

        // 5. Verificación: Persistencia tras recargar la página (cargado desde servidor)
        await Page.ReloadAsync();
        await Expect(favoriteBtn).ToHaveClassAsync(new System.Text.RegularExpressions.Regex(".*btn-danger.*"));
    }
}