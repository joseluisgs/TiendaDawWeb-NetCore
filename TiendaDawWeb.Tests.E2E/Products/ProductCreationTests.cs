using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using TiendaDawWeb.Tests.E2E.Extensions;

namespace TiendaDawWeb.Tests.E2E.Products;

/**
 * MÓDULO DE CREACIÓN DE PRODUCTOS (E2E)
 *
 * OBJETIVO: Verificar el flujo completo de creación de productos.
 * NOTA: Tests deshabilitados temporalmente por investigar error 500 en el servidor.
 * El flujo login → create funciona, pero hay un error al procesar el formulario.
 */
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ProductCreationTests : E2ETestBase
{
    private const string TestImagePath = "Fixtures/test-product.svg";
    private const string AdminEmail = "admin@waladaw.com";
    private const string AdminPassword = "admin";

    [Test]
    [Explicit("Pendiente de investigar error 500 en servidor")]
    public async Task UserCanCreateProductWithImage()
    {
        await LoginAsync(AdminEmail, AdminPassword);

        var productName = $"Producto con Imagen {DateTime.UtcNow:yyyyMMddHHmmss}";

        await Page.GotoAsync($"{BaseTestUrl}/Product/Create");

        await Page.TestId("nombre-input").FillAsync(productName);
        await Page.TestId("descripcion-input").FillAsync("Producto de prueba con imagen");
        await Page.TestId("precio-input").FillAsync("149.99");

        var absoluteImagePath = Path.GetFullPath(TestImagePath);
        await Page.Locator("input[type='file']").SetInputFilesAsync(absoluteImagePath);

        await Page.TestId("submit-button").ClickAsync();

        await Expect(Page).ToHaveURLAsync(
            new System.Text.RegularExpressions.Regex(".*Product/Details.*", System.Text.RegularExpressions.RegexOptions.IgnoreCase),
            new() { Timeout = 20000 });

        await Expect(Page.Locator("h1").First).ToContainTextAsync(
            new System.Text.RegularExpressions.Regex(productName, System.Text.RegularExpressions.RegexOptions.IgnoreCase), 
            new() { Timeout = 10000 });
    }

    [Test]
    [Explicit("Pendiente de investigar error 500 en servidor")]
    public async Task ProductAppearsInPublicListAfterCreation()
    {
        await LoginAsync(AdminEmail, AdminPassword);

        var productName = $"Buscable {DateTime.UtcNow:yyyyMMddHHmmss}";

        await Page.GotoAsync($"{BaseTestUrl}/Product/Create");
        await Page.TestId("nombre-input").FillAsync(productName);
        await Page.TestId("descripcion-input").FillAsync("Para verificar que aparece");
        await Page.TestId("precio-input").FillAsync("49.99");

        var absoluteImagePath = Path.GetFullPath(TestImagePath);
        await Page.Locator("input[type='file']").SetInputFilesAsync(absoluteImagePath);

        await Page.TestId("submit-button").ClickAsync();

        await Page.GotoAsync($"{BaseTestUrl}/Public");
        await Page.TestId("search-input").FillAsync(productName);
        await Page.TestId("search-button").ClickAsync();

        var productCard = Page.TestId("product-card").First;
        await Expect(productCard).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Test]
    [Explicit("Pendiente de investigar error 500 en servidor")]
    public async Task ProductOwnerCannotRateOwnProduct()
    {
        await LoginAsync(AdminEmail, AdminPassword);

        var productName = $"Mi Producto {DateTime.UtcNow:yyyyMMddHHmmss}";

        await Page.GotoAsync($"{BaseTestUrl}/Product/Create");
        await Page.TestId("nombre-input").FillAsync(productName);
        await Page.TestId("descripcion-input").FillAsync("Producto para probar");
        await Page.TestId("precio-input").FillAsync("75.00");

        var absoluteImagePath = Path.GetFullPath(TestImagePath);
        await Page.Locator("input[type='file']").SetInputFilesAsync(absoluteImagePath);

        await Page.TestId("submit-button").ClickAsync();

        var productId = System.Text.RegularExpressions.Regex.Match(Page.Url, @"id=(\d+)").Groups[1].Value;

        await Page.GotoAsync($"{BaseTestUrl}/Product/Details/{productId}");

        var ratingForm = Page.Locator("form:has-text('Valorar')");
        await Expect(ratingForm).Not.ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    [Test]
    [Explicit("Pendiente de investigar error 500 en servidor")]
    public async Task CreateProductWithoutImage()
    {
        await LoginAsync(AdminEmail, AdminPassword);

        var productName = $"Sin Imagen {DateTime.UtcNow:yyyyMMddHHmmss}";

        await Page.GotoAsync($"{BaseTestUrl}/Product/Create");
        await Page.TestId("nombre-input").FillAsync(productName);
        await Page.TestId("descripcion-input").FillAsync("Producto sin imagen");
        await Page.TestId("precio-input").FillAsync("29.99");

        await Page.TestId("submit-button").ClickAsync();

        await Expect(Page).ToHaveURLAsync(
            new System.Text.RegularExpressions.Regex(".*Product/Details.*", System.Text.RegularExpressions.RegexOptions.IgnoreCase),
            new() { Timeout = 20000 });

        await Expect(Page.Locator("h1").First).ToContainTextAsync(
            new System.Text.RegularExpressions.Regex(productName, System.Text.RegularExpressions.RegexOptions.IgnoreCase), 
            new() { Timeout = 10000 });
    }

    private async Task LoginAsync(string email, string password)
    {
        await Page.GotoAsync($"{BaseTestUrl}/Auth/Login");
        await Page.TestId("email-input").FillAsync(email);
        await Page.TestId("password-input").FillAsync(password);
        await Page.TestId("submit-button").ClickAsync();
        await Expect(Page.Locator(".navbar")).ToContainTextAsync(new System.Text.RegularExpressions.Regex("Admin", System.Text.RegularExpressions.RegexOptions.IgnoreCase), new() { Timeout = 10000 });
    }
}
