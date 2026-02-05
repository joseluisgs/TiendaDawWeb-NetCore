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
        
        await Page.TestId("search-input").FillAsync("iPhone 17 Pro Max");
        await Page.TestId("search-button").ClickAsync();
        await CaptureScreenshotAsync("04-search-results");

        var firstProductCard = Page.TestId("product-card").First;
        await Expect(firstProductCard).ToContainTextAsync("iPhone 17 Pro Max");

        await Page.TestId("product-title").First.ClickAsync();

        await Expect(Page.TestId("product-title")).ToContainTextAsync("iPhone 17 Pro Max");
        
        var sellerSection = Page.TestId("seller-info");
        await Expect(sellerSection).ToContainTextAsync("Prueba Probando Mucho");
        await Expect(Page.TestId("seller-email")).ToContainTextAsync("prueba@prueba.com");

        var addToCartBtn = Page.TestId("add-to-cart-button");
        await Expect(addToCartBtn).ToBeVisibleAsync();
        await Expect(addToCartBtn).ToBeEnabledAsync();
    }
}
