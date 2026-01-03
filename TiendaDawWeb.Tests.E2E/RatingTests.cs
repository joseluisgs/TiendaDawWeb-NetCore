using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace TiendaDawWeb.Tests.E2E;

/**
 * MÓDULO DE VALORACIONES (E2E - AJAX)
 * 
 * OBJETIVO: Validar que los usuarios pueden dejar opiniones y puntuaciones interactivas.
 * TECNOLOGÍAS TESTEADAS: JavaScript (ratings.js), Fetch API, SignalR (Notificaciones), DOM Manipulation.
 */
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class RatingTests : PageTest
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
    /// Test: Flujo de Valoración. Un usuario nuevo califica un producto con estrellas y comentario.
    /// </summary>
    [Test]
    public async Task AddRating_ShouldUpdateUiDynamically()
    {
        // 1. Acción: Registrar un usuario nuevo para tener permisos de valoración
        string id = System.Guid.NewGuid().ToString().Substring(0, 8);
        await Page.GotoAsync($"{BaseUrl}/Auth/Register");
        await Page.FillAsync("#Nombre", "Critic");
        await Page.FillAsync("#Apellidos", "E2E");
        await Page.FillAsync("#Email", $"critic_{id}@test.com");
        await Page.FillAsync("#Password", "Password123!");
        await Page.FillAsync("#ConfirmPassword", "Password123!");
        await Page.Locator(".card-body form button[type='submit']").ClickAsync();
        
        await Expect(Page.Locator(".navbar")).ToContainTextAsync("Critic");

        // 2. Acción: Navegar al detalle de un producto (ej. ID 1 - iPhone)
        await Page.GotoAsync($"{BaseUrl}/Product/Details/1");
        await Page.ScreenshotAsync(new PageScreenshotOptions { Path = "debug_rating_details.png" });
        
        // 3. Verificación: El formulario de valoración está visible
        var ratingForm = Page.Locator("#ratingForm");
        try {
            await Expect(ratingForm).ToBeVisibleAsync();
        } catch (Exception) {
            var content = await Page.TextContentAsync("#ratingSectionAJAX");
            Console.WriteLine("DEBUG Rating Section Content: " + content);
            throw;
        }

        // 4. Acción: Seleccionar 5 estrellas (clic en el icono con data-value="5")
        await Page.Locator(".star-item[data-value='5']").ClickAsync();

        // 5. Acción: Escribir comentario
        string comentario = "Excelente producto, probado por el test E2E " + id;
        await Page.FillAsync("textarea[name='Comentario']", comentario);

        // 6. Acción: Enviar valoración
        await Page.ClickAsync("#btnSubmitRating");

        // 7. Verificación: El formulario desaparece y muestra mensaje de agradecimiento (vía AJAX)
        await Expect(Page.Locator("#ratingSectionAJAX")).ToContainTextAsync(new System.Text.RegularExpressions.Regex("Gracias", System.Text.RegularExpressions.RegexOptions.IgnoreCase));

        // 8. Verificación: El comentario aparece en la lista inferior sin recargar la página
        var ratingsList = Page.Locator("#ratingsListAJAX");
        await Expect(ratingsList).ToContainTextAsync(comentario);
        await Expect(ratingsList).ToContainTextAsync("Critic");
    }
}
