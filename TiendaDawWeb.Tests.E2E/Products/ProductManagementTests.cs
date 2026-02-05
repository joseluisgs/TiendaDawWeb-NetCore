using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using System.IO;
using TiendaDawWeb.Tests.E2E.Extensions;

namespace TiendaDawWeb.Tests.E2E.Products;

/**
 * MÓDULO DE GESTIÓN DE PRODUCTOS (E2E)
 * 
 * OBJETIVO: Probar la edición de entidades y el servicio de almacenamiento (IStorageService).
 * TECNOLOGÍAS TESTEADAS: IFormFile (Subida de archivos), Playwright SetInputFiles, Locale es-ES.
 */
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ProductManagementTests : E2ETestBase
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

    [SetUp]
    public async Task Setup()
    {
        try
        {
            await Page.GotoAsync($"{BaseTestUrl}/Auth/Login", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
            await CaptureScreenshotAsync("01-login-page");
            
            await Page.TestId("email-input").FillAsync("prueba@prueba.com");
            await Page.TestId("password-input").FillAsync("prueba");
            await CaptureScreenshotAsync("02-credentials-filled");
            
            await Page.TestId("submit-button").ClickAsync();
            await CaptureScreenshotAsync("03-after-login");

            await Expect(Page.TestId("user-name")).ToContainTextAsync(new System.Text.RegularExpressions.Regex("Prueba", System.Text.RegularExpressions.RegexOptions.IgnoreCase), new() { Timeout = 10000 });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR en Setup: {ex.Message}");
            await Page.ScreenshotAsync(new() { Path = "setup-failure.png" });
            throw;
        }
    }

    [Test]
    public async Task EditProduct_ShouldUpdateValuesAndUploadImage()
    {
        try
        {
            await Page.GotoAsync($"{BaseTestUrl}/Product/MyProducts", new() { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 15000 });
            await CaptureScreenshotAsync("04-my-products");

            await Expect(Page.Locator("main")).ToBeVisibleAsync(new() { Timeout = 10000 });

            var editButtons = Page.Locator("a.btn-warning");
            var count = await editButtons.CountAsync();
            Assert.That(count, Is.GreaterThan(0), "No hay productos disponibles para editar");

            var firstEditBtn = editButtons.First;
            await Expect(firstEditBtn).ToBeVisibleAsync(new() { Timeout = 10000 });
            await firstEditBtn.ClickAsync();
            await CaptureScreenshotAsync("05-edit-form");

            await Expect(Page.Locator("#Descripcion")).ToBeVisibleAsync(new() { Timeout = 10000 });

            string nuevaDesc = "Descripción generada por Playwright " + System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
            await Page.FillAsync("#Descripcion", nuevaDesc);
            await Page.FillAsync("#Precio", "125.50");
            await CaptureScreenshotAsync("06-filled-form");

            var projectRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".."));
            var fixturePath = Path.Combine(projectRoot, "Fixtures", "test-product.svg");
            Assert.That(File.Exists(fixturePath), Is.True, $"El fixture no existe en: {fixturePath}");
            await Page.SetInputFilesAsync("#ImagenFile", fixturePath);

            await Page.ClickAsync("main form button[type='submit']");
            await CaptureScreenshotAsync("07-after-submit");

            await Expect(Page.Locator("body")).ToContainTextAsync(new System.Text.RegularExpressions.Regex("exitosamente|actualizado", System.Text.RegularExpressions.RegexOptions.IgnoreCase), new() { Timeout = 15000 });
            await CaptureScreenshotAsync("08-success-message");

            await Expect(Page.Locator("main")).ToContainTextAsync(nuevaDesc, new() { Timeout = 5000 });
            await Expect(Page.Locator("main")).ToContainTextAsync(new System.Text.RegularExpressions.Regex("125[.,]50"), new() { Timeout = 5000 });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR en EditProduct: {ex.Message}");
            await Page.ScreenshotAsync(new() { Path = "edit-product-failure.png" });
            throw;
        }
    }
}