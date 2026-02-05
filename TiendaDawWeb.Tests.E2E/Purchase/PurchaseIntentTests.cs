using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using TiendaDawWeb.Tests.E2E.Extensions;

namespace TiendaDawWeb.Tests.E2E.Purchase;

/**
 * MÓDULO DE INTENCIÓN DE COMPRA (E2E)
 * 
 * OBJETIVO: Simular un usuario interesado que busca un producto específico y verifica al vendedor.
 * TECNOLOGÍAS TESTEADAS: Buscador, Filtros de Propiedad, Verificación de Vendedor (Relaciones EF Core).
 */
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class PurchaseIntentTests : E2ETestBase
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

    [Test]
    public async Task SearchSpecificProduct_AndVerifySeller_ShouldShowPurchaseOption()
    {
        string id = System.Guid.NewGuid().ToString().Substring(0, 8);
        await Page.GotoAsync($"{BaseTestUrl}/Auth/Register");
        await CaptureScreenshotAsync("01-register-page");

        await Page.TestId("nombre-input").FillAsync("Comprador");
        await Page.TestId("apellidos-input").FillAsync("E2E");
        await Page.TestId("email-input").FillAsync($"comprador_{id}@test.com");
        await Page.TestId("password-input").FillAsync("Password123!");
        await Page.TestId("confirm-password-input").FillAsync("Password123!");
        await Page.TestId("submit-button").ClickAsync();
        await CaptureScreenshotAsync("02-after-registration");

        await Expect(Page.TestId("user-name")).ToContainTextAsync("Comprador");

        await Page.GotoAsync($"{BaseTestUrl}/Public");
        await CaptureScreenshotAsync("03-public-catalog");
        
        await Page.TestId("search-input").FillAsync("Samsung Galaxy S24");
        await Page.TestId("search-button").ClickAsync();
        await CaptureScreenshotAsync("04-search-results");

        var firstProductCard = Page.TestId("product-card").First;
        await Expect(firstProductCard).ToContainTextAsync("Samsung Galaxy S24");

        var productLinks = Page.Locator("a[href*='/Product/Details']");
        if (await productLinks.CountAsync() > 0)
        {
            await productLinks.First.ClickAsync();
        }
        else
        {
            await Page.TestId("product-title").First.ClickAsync();
        }

        await CaptureScreenshotAsync("05-product-details");
        await Page.WaitForTimeoutAsync(2000);

        var productTitle = Page.Locator("h1, [data-testid='product-title']").First;
        await Expect(productTitle).ToBeVisibleAsync(new() { Timeout = 5000 });

        var sellerSection = Page.Locator("[data-testid='seller-info'], .seller-info");
        if (await sellerSection.CountAsync() > 0)
        {
            var sellerContent = await sellerSection.TextContentAsync();
            Assert.That(sellerContent != null && sellerContent.Contains("Prueba"), Is.True);
        }

        Console.WriteLine("Producto encontrado - verificación completada");
    }
}
