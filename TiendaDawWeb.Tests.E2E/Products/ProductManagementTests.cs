using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using System.IO;
using TiendaDawWeb.Tests.E2E.Extensions;

namespace TiendaDawWeb.Tests.E2E.Products;

/**
 * MÓDULO DE GESTIÓN DE PRODUCTOS (E2E) - Framework Agnóstico
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
        await Page.GotoAsync($"{BaseTestUrl}/Auth/Login", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await CaptureScreenshotAsync("01-login-page");

        await Page.TestId("email-input").FillAsync("prueba@prueba.com");
        await Page.TestId("password-input").FillAsync("prueba");
        await CaptureScreenshotAsync("02-credentials-filled");

        await Page.TestId("submit-button").ClickAsync();
        await CaptureScreenshotAsync("03-after-login");

        await Expect(Page.TestId("user-name")).ToContainTextAsync(new System.Text.RegularExpressions.Regex("Prueba", System.Text.RegularExpressions.RegexOptions.IgnoreCase), new() { Timeout = 10000 });
    }

    [Test]
    public async Task EditProduct_ShouldUpdateValues()
    {
        await Page.GotoAsync($"{BaseTestUrl}/Product/MyProducts", new() { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 15000 });
        await CaptureScreenshotAsync("04-my-products");

        var editButtons = Page.Locator("a:has-text('Editar'), a.btn-warning");
        var count = await editButtons.CountAsync();
        Assert.That(count, Is.GreaterThan(0), "No hay productos disponibles para editar");

        await editButtons.First.ClickAsync();
        await CaptureScreenshotAsync("05-edit-form");

        var descripcionInput = Page.Locator("input[name*='Descripcion'], textarea, #Descripcion").First;
        await Expect(descripcionInput).ToBeVisibleAsync(new() { Timeout = 10000 });

        string nuevaDesc = "Test " + System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
        await descripcionInput.FillAsync(nuevaDesc);

        var precioInput = Page.Locator("input[name*='Precio'], #Precio").First;
        await precioInput.FillAsync("125.50");
        await CaptureScreenshotAsync("06-filled-form");

        await Page.ClickAsync("button[type='submit']");
        await CaptureScreenshotAsync("07-after-submit");

        await Expect(Page.Locator("body")).ToContainTextAsync(new System.Text.RegularExpressions.Regex("exitosamente|actualizado|guardado", System.Text.RegularExpressions.RegexOptions.IgnoreCase), new() { Timeout = 15000 });
        await CaptureScreenshotAsync("08-success-message");
    }
}
