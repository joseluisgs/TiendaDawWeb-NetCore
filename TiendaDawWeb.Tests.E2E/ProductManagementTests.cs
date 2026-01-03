using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using System.IO;

namespace TiendaDawWeb.Tests.E2E;

/**
 * MÓDULO DE GESTIÓN DE PRODUCTOS (E2E)
 * 
 * OBJETIVO: Probar la edición de entidades y el servicio de almacenamiento (IStorageService).
 * TECNOLOGÍAS TESTEADAS: IFormFile (Subida de archivos), Playwright SetInputFiles, Locale es-ES.
 */
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ProductManagementTests : PageTest
{
    private const string BaseUrl = "http://localhost:5000";

    /// <summary>
    /// Configuración del contexto: Forzamos locale español para validar formatos decimales.
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
    /// Configuración previa: Login como usuario propietario.
    /// </summary>
    [SetUp]
    public async Task Setup()
    {
        try
        {
            await Page.GotoAsync($"{BaseUrl}/Auth/Login", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
            await Page.FillAsync("#Email", "prueba@prueba.com");
            await Page.FillAsync("#Password", "prueba");
            await Page.ClickAsync(".card-body form button[type='submit']");

            // Espera explícita de navegación tras el submit
            // await Page.WaitForLoadStateAsync(LoadState.NetworkIdle); // OPTIMIZADO: No es necesario esperar explícitamente

            // Espera que la barra de navegación muestre el nombre del usuario logueado.
            await Expect(Page.Locator(".navbar")).ToContainTextAsync(new System.Text.RegularExpressions.Regex("Prueba", System.Text.RegularExpressions.RegexOptions.IgnoreCase), new() { Timeout = 10000 });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR en Setup: {ex.Message}");
            await Page.ScreenshotAsync(new() { Path = "setup-failure.png" });
            throw;
        }
    }

    /// <summary>
    /// Test: Edición completa. Permite cambiar datos y subir una imagen (fixture local).
    /// Incluye comentarios para guiar al alumnado sobre buenas prácticas Playwright E2E.
    /// </summary>
    [Test]
    public async Task EditProduct_ShouldUpdateValuesAndUploadImage()
    {
        try
        {
            // 1. Acción: Ir a Mis Productos y verificar que hay productos disponibles
            await Page.GotoAsync($"{BaseUrl}/Product/MyProducts", new() { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 15000 });

            // Esperar a que el contenedor principal sea visible
            await Expect(Page.Locator("main")).ToBeVisibleAsync(new() { Timeout = 10000 });

            // Verificar que hay productos antes de intentar editar
            var editButtons = Page.Locator("a.btn-warning");
            var count = await editButtons.CountAsync();
            Assert.That(count, Is.GreaterThan(0), "No hay productos disponibles para editar");

            // 2. Acción: Seleccionar el primer botón de edición
            var firstEditBtn = editButtons.First;
            await Expect(firstEditBtn).ToBeVisibleAsync(new() { Timeout = 10000 });
            await firstEditBtn.ClickAsync();

            // Esperar a que el formulario de edición sea visible (más fiable que NetworkIdle)
            await Expect(Page.Locator("#Descripcion")).ToBeVisibleAsync(new() { Timeout = 10000 });

            // 3. Acción: Modificar campos de texto y precio con un valor único (timestamp).
            string nuevaDesc = "Descripción generada por Playwright " + System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
            await Page.FillAsync("#Descripcion", nuevaDesc);
            await Page.FillAsync("#Precio", "125.50");

            // 4. Acción: Subida de archivo usando fixture local.
            var projectRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".."));
            var fixturePath = Path.Combine(projectRoot, "Fixtures", "test-product.svg");
            Assert.That(File.Exists(fixturePath), Is.True, $"El fixture no existe en: {fixturePath}");
            await Page.SetInputFilesAsync("#ImagenFile", fixturePath);

            // 5. Acción: Guardar cambios (submit del formulario).
            await Page.ClickAsync("main form button[type='submit']");

            // 6. Verificación: El mensaje de éxito debe aparecer (esto confirma que la navegación terminó).
            await Expect(Page.Locator("body")).ToContainTextAsync(new System.Text.RegularExpressions.Regex("exitosamente|actualizado", System.Text.RegularExpressions.RegexOptions.IgnoreCase), new() { Timeout = 15000 });

            // 7. Verificación: Persistencia del valor editado en la ficha de producto.
            await Expect(Page.Locator("body")).ToContainTextAsync(new System.Text.RegularExpressions.Regex("exitosamente|actualizado", System.Text.RegularExpressions.RegexOptions.IgnoreCase), new() { Timeout = 5000 });

            // 8. Verificación: Persistencia del valor editado en la ficha de producto.
            await Expect(Page.Locator("main")).ToContainTextAsync(nuevaDesc, new() { Timeout = 5000 });

            // 9. Verificación: Formato decimal correcto (con coma para español o punto, según el render).
            await Expect(Page.Locator("main")).ToContainTextAsync(new System.Text.RegularExpressions.Regex("125[.,]50"), new() { Timeout = 5000 });

            /*
             * Comentario para el alumnado:
             * Esperar a la navegación tras el submit es crucial en E2E con Playwright.
             * Si se hace el assertion demasiado rápido, la página puede estar mostrando solo el mensaje de éxito,
             * pero no el dato actualizado. La buena práctica es usar WaitForURLAsync o navegar manualmente.
             * Así evitamos falsos positivos y comprobamos que realmente el flujo es el esperado.
             */
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR en EditProduct: {ex.Message}");
            await Page.ScreenshotAsync(new() { Path = "edit-product-failure.png" });
            throw;
        }
    }
}