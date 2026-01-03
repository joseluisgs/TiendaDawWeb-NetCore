using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace TiendaDawWeb.Tests.E2E;

/**
 * MÓDULO DE VALORACIONES BLAZOR (E2E)
 * 
 * OBJETIVO: Validar la interactividad de los componentes Blazor Server y la persistencia de votos.
 * TECNOLOGÍAS TESTEADAS: Blazor Server (SignalR), StateContainer, RenderFragments.
 */
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class BlazorRatingTests : PageTest
{
    private const string BaseUrl = "http://localhost:5000";

    public override BrowserNewContextOptions ContextOptions()
    {
        return new BrowserNewContextOptions
        {
            Locale = "es-ES",
            TimezoneId = "Europe/Madrid"
        };
    }

    /// <summary>
    /// Test: Ciclo de vida de una valoración Blazor con usuario fresco.
    /// </summary>
    [Test]
    public async Task SubmitBlazorRating_ShouldUpdateAndPersist()
    {
        // 1. Registro de un usuario único para asegurar que no ha votado antes
        string id = System.Guid.NewGuid().ToString().Substring(0, 8);
        await Page.GotoAsync($"{BaseUrl}/Auth/Register");
        await Page.FillAsync("#Nombre", "Critic" + id);
        await Page.FillAsync("#Apellidos", "E2E");
        await Page.FillAsync("#Email", $"critic_{id}@test.com");
        await Page.FillAsync("#Password", "Password123!");
        await Page.FillAsync("#ConfirmPassword", "Password123!");
        await Page.Locator(".card-body form button[type='submit']").ClickAsync();
        
        await Expect(Page.Locator(".navbar")).ToContainTextAsync("Critic" + id);

        // 2. Ir al detalle del producto 42
        await Page.GotoAsync($"{BaseUrl}/Product/Details/42");

        // 3. Verificación: El componente Blazor se ha cargado (Formulario)
        var ratingSection = Page.Locator("#cardFormulario");
        await Expect(ratingSection).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = 15000 });
        
        await Page.ScreenshotAsync(new PageScreenshotOptions { Path = "debug_blazor_stars.png" });
        var stars = Page.Locator(".star-item");
        await Expect(stars.First).ToBeVisibleAsync();

        // 4. Seleccionar 4 estrellas (Índice 3)
        await stars.Nth(3).ClickAsync();
        await Page.ScreenshotAsync(new PageScreenshotOptions { Path = "debug_after_star_click.png" });

        // 5. Escribir comentario y enviar
        string comentario = "el mejor producto " + id;
        await Page.Locator("textarea").FillAsync(comentario);
        await Page.ClickAsync("button:has-text('Enviar Valoración')");

        // 6. Verificación (Blazor): La UI cambia a la tarjeta de gracias
        await Expect(Page.Locator("body")).ToContainTextAsync(new System.Text.RegularExpressions.Regex("¡Gracias por tu valoración!", System.Text.RegularExpressions.RegexOptions.IgnoreCase));
        
        // 7. Verificación (Persistencia): F5
        await Page.ReloadAsync();
        await Expect(Page.Locator("body")).ToContainTextAsync("¡Gracias por tu valoración!");
        await Expect(Page.Locator("main")).ToContainTextAsync(comentario);
    }
}