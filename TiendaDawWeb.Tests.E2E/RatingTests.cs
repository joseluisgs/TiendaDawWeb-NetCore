using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace TiendaDawWeb.Tests.E2E;

/// <summary>
/// MÓDULO DE VALORACIONES (Ratings) - E2E
///
/// OBJETIVO: Probar el flujo completo de valoración de productos con Blazor Server.
/// TECNOLOGÍAS TESTEADAS: Blazor Server Components, SignalR, RatingService, StateContainer.
/// </summary>
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class RatingTests : PageTest
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
    /// Test: Flujo completo de valoración de producto.
    /// OBJETIVO: Validar que un usuario puede valorar un producto y ver los cambios persistir.
    /// PASOS:
    /// 1. Login como usuario
    /// 2. Buscar un producto que no sea del propio usuario
    /// 3. Entrar al producto
    /// 4. Seleccionar 4 estrellas
    /// 5. Escribir comentario "test"
    /// 6. Enviar valoración
    /// 7. Verificar que aparece "¡Gracias por tu valoración!"
    /// 8. Verificar que se actualiza el promedio de ratings
    /// 9. Recargar y verificar que persiste el mensaje de gracias
    /// </summary>
    [Test]
    public async Task SubmitRating_ShouldUpdateDisplayAndPersist()
    {
        try
        {
            // 1. Login como usuario "carlos" (no admin, no María, no otro)
            await Page.GotoAsync($"{BaseUrl}/Auth/Login", new() { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 15000 });
            await Page.FillAsync("#Email", "carlos@email.com");
            await Page.FillAsync("#Password", "carlos123");
            await Page.ClickAsync(".card-body form button[type='submit']");

            // Esperar navegación tras login
            await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*"), new() { Timeout = 10000 });
            await Expect(Page.Locator(".navbar")).ToContainTextAsync(new System.Text.RegularExpressions.Regex("Carlos", System.Text.RegularExpressions.RegexOptions.IgnoreCase), new() { Timeout = 10000 });

            // 2. Ir a un producto específico que NO sea del usuario "carlos"
            // Usamos el iPhone 17 Pro Max (ID 1, propiedad del usuario 'prueba', no de "carlos")
            await Page.GotoAsync($"{BaseUrl}/Product/Details/1", new() { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 15000 });

            // Esperar que la página de detalles cargue
            await Expect(Page.Locator("h1")).ToBeVisibleAsync(new() { Timeout = 10000 });

            // Obtener el producto actual
            var productName = await Page.Locator("h1").TextContentAsync();
            Console.WriteLine($"Probando valoración del producto: {productName}");

            // 3. Verificar que el formulario de valoración es visible
            var ratingForm = Page.Locator("#cardFormulario");
            await Expect(ratingForm).ToBeVisibleAsync(new() { Timeout = 10000 });

            // 4. Seleccionar 4 estrellas
            var stars = Page.Locator(".star-rating-input .star-item");
            await Expect(stars.Nth(0)).ToBeVisibleAsync(new() { Timeout = 10000 });

            // Hacer click en la 4ta estrella
            await stars.Nth(3).ClickAsync();

            // Esperar que la estrella se seleccione (clase text-warning)
            await Expect(stars.Nth(3)).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-warning"), new() { Timeout = 5000 });

            // 5. Escribir comentario "test"
            var commentInput = Page.Locator("textarea[placeholder*='Qué te ha parecido']");
            await Expect(commentInput).ToBeVisibleAsync(new() { Timeout = 5000 });
            await commentInput.FillAsync("test");

            // 6. Enviar valoración
            var submitButton = Page.Locator("button[type='submit']:has-text('Enviar Valoración')");
            await Expect(submitButton).ToBeVisibleAsync(new() { Timeout = 5000 });
            await submitButton.ClickAsync();

            // Esperar a que aparezca el mensaje de éxito o el mensaje de gracias
            // El mensaje de éxito puede ser temporal, así que esperamos el mensaje de gracias
            Console.WriteLine("Esperando actualización tras enviar valoración...");

            // Esperar a que desaparezca el formulario y aparezca el mensaje de gracias
            await Task.Delay(2000); // Dar tiempo a Blazor para actualizar

            // 7. Verificar que aparece "¡Gracias por tu valoración!"
            var thanksMessage = Page.Locator("text=/Gracias por tu valoración/i");
            await Expect(thanksMessage).ToBeVisibleAsync(new() { Timeout = 15000 });

            // Verificar que el mensaje muestra 4 estrellas (texto "4 / 5")
            var userRatingSection = thanksMessage.Locator("..").Locator("..");
            await Expect(userRatingSection).ToContainTextAsync("4 / 5", new() { Timeout = 5000 });
            Console.WriteLine("✓ Verificado: La valoración muestra 4 estrellas");

            // Verificar que el comentario "test" aparece
            await Expect(userRatingSection).ToContainTextAsync("test", new() { Timeout = 5000 });
            Console.WriteLine("✓ Verificado: El comentario 'test' aparece");

            // 8. Verificar que se actualiza la sección de valoraciones (aparece la nueva valoración en el listado)
            await Task.Delay(1000); // Dar tiempo para que se actualice

            var ratingsList = Page.Locator("#ratingsList");
            await Expect(ratingsList).ToContainTextAsync("Carlos", new() { Timeout = 5000 });
            await Expect(ratingsList).ToContainTextAsync("test", new() { Timeout = 5000 });
            Console.WriteLine("✓ Verificado: La valoración aparece en el listado");

            // 9. Recargar la página y verificar que persiste el mensaje de gracias
            await Page.ReloadAsync(new() { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 15000 });

            // Esperar a que cargue la página
            await Expect(Page.Locator("h1")).ToBeVisibleAsync(new() { Timeout = 10000 });

            // Verificar que sigue mostrando "¡Gracias por tu valoración!"
            var thanksMessageReload = Page.Locator("text=/Gracias por tu valoración/i");
            await Expect(thanksMessageReload).ToBeVisibleAsync(new() { Timeout = 10000 });

            // Verificar que el comentario persiste
            var userRatingSectionReload = thanksMessageReload.Locator("..").Locator("..");
            await Expect(userRatingSectionReload).ToContainTextAsync("test", new() { Timeout = 5000 });

            // Verificar que las 4 estrellas persisten después de recargar
            await Expect(userRatingSectionReload).ToContainTextAsync("4 / 5", new() { Timeout = 5000 });
            Console.WriteLine("✓ Verificado: La valoración de 4 estrellas persiste tras recargar");

            Console.WriteLine("✅ Test de valoración completado exitosamente");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR en SubmitRating: {ex.Message}");
            await Page.ScreenshotAsync(new() { Path = "rating-test-failure.png" });
            throw;
        }
    }
}
