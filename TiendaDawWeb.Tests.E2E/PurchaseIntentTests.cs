using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace TiendaDawWeb.Tests.E2E;

/**
 * MÓDULO DE INTENCIÓN DE COMPRA (E2E)
 * 
 * OBJETIVO: Simular un usuario interesado que busca un producto específico y verifica al vendedor.
 * TECNOLOGÍAS TESTEADAS: Buscador, Filtros de Propiedad, Verificación de Vendedor (Relaciones EF Core).
 */
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class PurchaseIntentTests : PageTest
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
    /// Test: Búsqueda y Verificación. El usuario busca un iPhone específico y comprueba que el vendedor es el correcto.
    /// </summary>
    [Test]
    public async Task SearchSpecificProduct_AndVerifySeller_ShouldShowPurchaseOption()
    {
        // 1. Acción: Registrar un nuevo usuario comprador para tener un estado limpio
        string id = System.Guid.NewGuid().ToString().Substring(0, 8);
        await Page.GotoAsync($"{BaseUrl}/Auth/Register");
        await Page.FillAsync("#Nombre", "Comprador");
        await Page.FillAsync("#Apellidos", "E2E");
        await Page.FillAsync("#Email", $"comprador_{id}@test.com");
        await Page.FillAsync("#Password", "Password123!");
        await Page.FillAsync("#ConfirmPassword", "Password123!");
        await Page.Locator(".card-body form button[type='submit']").ClickAsync();
        
        await Expect(Page.Locator(".navbar")).ToContainTextAsync("Comprador");

        // 2. Acción: Buscar el producto exacto "iPhone 17 Pro Max"
        await Page.GotoAsync($"{BaseUrl}/Public");
        // Hay dos buscadores (Navbar y Main), usamos el de Main para el test
        await Page.Locator("main input[name='q']").FillAsync("iPhone 17 Pro Max");
        await Page.ClickAsync("main button[type='submit']");

        // 3. Verificación: El primer resultado es el iPhone deseado
        var firstProductCard = Page.Locator(".producto-card").First;
        await Expect(firstProductCard).ToContainTextAsync("iPhone 17 Pro Max");

        // 4. Acción: Entrar al detalle del producto
        await firstProductCard.Locator("a").First.ClickAsync();

        // 5. Verificación: El título es correcto y el vendedor es "Prueba Probando Mucho"
        await Expect(Page.Locator("h1")).ToContainTextAsync("iPhone 17 Pro Max");
        
        var vendorSection = Page.Locator(".card:has-text('Vendedor')");
        await Expect(vendorSection).ToContainTextAsync("Prueba Probando Mucho");
        await Expect(vendorSection).ToContainTextAsync("prueba@prueba.com");

        // 6. Verificación: El botón de compra está habilitado (no somos los dueños)
        var addToCartBtn = Page.Locator("button:has-text('Añadir al Carrito')");
        await Expect(addToCartBtn).ToBeVisibleAsync();
        await Expect(addToCartBtn).ToBeEnabledAsync();
    }
}
