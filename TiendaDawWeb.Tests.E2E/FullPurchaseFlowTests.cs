using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace TiendaDawWeb.Tests.E2E;

/**
 * MÓDULO DE FLUJO COMPLETO DE COMPRA (E2E)
 * 
 * OBJETIVO: Validar el ciclo de vida completo desde el registro hasta la obtención de factura.
 * TECNOLOGÍAS TESTEADAS: CarritoService, PurchaseService, Generación de PDF, ASP.NET Core Identity.
 */
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class FullPurchaseFlowTests : PageTest
{
    private const string BaseUrl = "http://localhost:5000";

    public override BrowserNewContextOptions ContextOptions()
    {
        return new BrowserNewContextOptions
        {
            Locale = "es-ES",
            TimezoneId = "Europe/Madrid",
            AcceptDownloads = true 
        };
    }

    /// <summary>
    /// Test: Flujo completo de compra. Registro -> Búsqueda -> Carrito -> Compra -> Factura.
    /// </summary>
    [Test]
    public async Task CompletePurchaseFlow_ShouldSucceed()
    {
        // 1. Registro de usuario único
        string id = System.Guid.NewGuid().ToString().Substring(0, 8);
        string email = $"buyer_{id}@test.com";
        await Page.GotoAsync($"{BaseUrl}/Auth/Register");
        await Page.FillAsync("#Nombre", "Comprador");
        await Page.FillAsync("#Apellidos", "E2E");
        await Page.FillAsync("#Email", email);
        await Page.FillAsync("#Password", "Password123!");
        await Page.FillAsync("#ConfirmPassword", "Password123!");
        await Page.ClickAsync("button[type='submit']");
        
        await Expect(Page.Locator(".navbar")).ToContainTextAsync("Comprador");

        // 2. Búsqueda de un producto seguro (iPhone del SeedData)
        await Page.GotoAsync($"{BaseUrl}/Public");
        await Page.Locator("main input[name='q']").FillAsync("iPhone 17 Pro Max");
        await Page.ClickAsync("main button[type='submit']");

        // 3. Navegación al detalle del primer resultado
        var firstProductCard = Page.Locator(".producto-card").First;
        await Expect(firstProductCard).ToBeVisibleAsync();
        await firstProductCard.Locator("a").First.ClickAsync();

        // 4. Añadir al Carrito (Selector robusto por formulario)
        // En Details.cshtml el botón está dentro de un form con acción AddToCart
        var addToCartForm = Page.Locator("form[action*='AddToCart'], form[action*='Carrito/Add']");
        var addToCartBtn = addToCartForm.Locator("button[type='submit']");
        await Expect(addToCartBtn).ToBeVisibleAsync();
        await addToCartBtn.ClickAsync();

        // 5. Verificar mensaje de éxito y navegar al carrito
        await Expect(Page.Locator("body")).ToContainTextAsync(new System.Text.RegularExpressions.Regex("añadido|éxito", System.Text.RegularExpressions.RegexOptions.IgnoreCase));
        await Page.GotoAsync($"{BaseUrl}/app/carrito");

        // 6. Finalizar Compra
        // Buscamos el botón de finalizar (usamos selector por texto o clase de éxito)
        var checkoutBtn = Page.Locator("button.btn-success:has-text('Finalizar'), button:has-text('Compra')").First;
        await Expect(checkoutBtn).ToBeVisibleAsync();
        await checkoutBtn.ClickAsync();

        // 7. Verificación de página de confirmación
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*/Purchase/Confirmacion/.*"));
        await Expect(Page.Locator("body")).ToContainTextAsync(new System.Text.RegularExpressions.Regex("éxito|gracias", System.Text.RegularExpressions.RegexOptions.IgnoreCase));

        // 8. Ir a Mis Compras y Descargar Factura
        await Page.GotoAsync($"{BaseUrl}/Purchase/MyPurchases");
        
        var downloadTask = Page.WaitForDownloadAsync();
        // Buscamos el enlace que contenga 'Factura' o el icono de archivo
        await Page.Locator("a:has-text('Factura'), a .bi-file-earmark-pdf").First.ClickAsync();
        var download = await downloadTask;

        // Verificación final del archivo
        Assert.That(download.SuggestedFilename, Does.EndWith(".pdf"));
        Console.WriteLine($"SUCCESS: Flujo completo verificado. Factura: {download.SuggestedFilename}");
    }
}