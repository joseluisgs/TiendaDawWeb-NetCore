using System.Text.RegularExpressions;
using Microsoft.Playwright;
using TiendaDawWeb.Tests.E2E.Extensions;

namespace TiendaDawWeb.Tests.E2E.Products;

/**
 * MÓDULO DE CREACIÓN DE PRODUCTOS (E2E)
 *
 * OBJETIVO: Verificar el flujo completo de creación de productos.
 * ESTRATEGIA:
 * - Login con admin@waladaw.com
 * - Crear producto con datos válidos
 * - Verificar redirección a Details
 * - Framework agnóstico: funciona en MVC y Razor Pages
 */
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ProductCreationTests : E2ETestBase {
    [Test]
    public async Task UserCanCreateProductWithImage() {
        await LoginAsync(AdminEmail, AdminPassword);

        string productName = $"Producto con Imagen {DateTime.UtcNow:yyyyMMddHHmmss}";

        await Page.GotoAsync($"{BaseTestUrl}/Product/Create");

        await Page.TestId("nombre-input").FillAsync(productName);
        await Page.TestId("descripcion-input").FillAsync("Producto de prueba con imagen");
        await Page.TestId("precio-input").FillAsync("149.99");
        await Page.Locator("select[name*='Categoria'], select[id*='Categoria']").SelectOptionAsync("SMARTPHONES");

        string projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Fixtures",
            "test-product.svg"));
        if (File.Exists(projectRoot)) {
            await Page.Locator("input[type='file']").SetInputFilesAsync(projectRoot);
        }

        await Page.TestId("submit-button").ClickAsync();

        await Expect(Page).ToHaveURLAsync(
            new Regex(".*Product/Details.*", RegexOptions.IgnoreCase),
            new PageAssertionsToHaveURLOptions { Timeout = 20000 });

        await Expect(Page.Locator("h1").First).ToContainTextAsync(
            new Regex(productName, RegexOptions.IgnoreCase),
            new LocatorAssertionsToContainTextOptions { Timeout = 10000 });
    }

    [Test]
    public async Task ProductAppearsInPublicListAfterCreation() {
        await LoginAsync(AdminEmail, AdminPassword);

        string productName = $"Buscable {DateTime.UtcNow:yyyyMMddHHmmss}";

        await Page.GotoAsync($"{BaseTestUrl}/Product/Create");
        await Page.TestId("nombre-input").FillAsync(productName);
        await Page.TestId("descripcion-input").FillAsync("Para verificar que aparece");
        await Page.TestId("precio-input").FillAsync("49.99");
        await Page.Locator("select[name*='Categoria'], select[id*='Categoria']").SelectOptionAsync("ACCESSORIES");

        string projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Fixtures",
            "test-product.svg"));
        if (File.Exists(projectRoot)) {
            await Page.Locator("input[type='file']").SetInputFilesAsync(projectRoot);
        }

        await Page.TestId("submit-button").ClickAsync();

        await Page.GotoAsync($"{BaseTestUrl}/Public");
        await Page.TestId("search-input").FillAsync(productName);
        await Page.TestId("search-button").ClickAsync();

        ILocator productCard = Page.TestId("product-card").First;
        await Expect(productCard).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = 10000 });
    }

    [Test]
    public async Task ProductOwnerCannotRateOwnProduct() {
        await LoginAsync(AdminEmail, AdminPassword);

        string productName = $"Mi Producto {DateTime.UtcNow:yyyyMMddHHmmss}";

        await Page.GotoAsync($"{BaseTestUrl}/Product/Create");
        await Page.TestId("nombre-input").FillAsync(productName);
        await Page.TestId("descripcion-input").FillAsync("Producto para probar");
        await Page.TestId("precio-input").FillAsync("75.00");
        await Page.Locator("select[name*='Categoria'], select[id*='Categoria']").SelectOptionAsync("SMARTPHONES");

        string projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Fixtures",
            "test-product.svg"));
        if (File.Exists(projectRoot)) {
            await Page.Locator("input[type='file']").SetInputFilesAsync(projectRoot);
        }

        await Page.TestId("submit-button").ClickAsync();

        string productId = Regex.Match(Page.Url, @"id=(\d+)").Groups[1].Value;

        await Page.GotoAsync($"{BaseTestUrl}/Product/Details/{productId}");

        ILocator ratingForm = Page.Locator("form:has-text('Valorar')");
        await Expect(ratingForm).Not.ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = 5000 });
    }

    [Test]
    public async Task CreateProductWithoutImage() {
        await LoginAsync(AdminEmail, AdminPassword);

        string productName = $"Sin Imagen {DateTime.UtcNow:yyyyMMddHHmmss}";

        await Page.GotoAsync($"{BaseTestUrl}/Product/Create");
        await Page.TestId("nombre-input").FillAsync(productName);
        await Page.TestId("descripcion-input").FillAsync("Producto sin imagen");
        await Page.TestId("precio-input").FillAsync("29.99");
        await Page.Locator("select[name*='Categoria'], select[id*='Categoria']").SelectOptionAsync("SMARTPHONES");

        await Page.TestId("submit-button").ClickAsync();

        await Expect(Page).ToHaveURLAsync(
            new Regex(".*Product/Details.*", RegexOptions.IgnoreCase),
            new PageAssertionsToHaveURLOptions { Timeout = 20000 });

        await Expect(Page.Locator("h1").First).ToContainTextAsync(
            new Regex(productName, RegexOptions.IgnoreCase),
            new LocatorAssertionsToContainTextOptions { Timeout = 10000 });
    }

    private async Task LoginAsync(string email, string password) {
        await Page.GotoAsync($"{BaseTestUrl}/Auth/Login");
        await Page.TestId("email-input").FillAsync(email);
        await Page.TestId("password-input").FillAsync(password);
        await Page.TestId("submit-button").ClickAsync();
        await Expect(Page.Locator(".navbar")).ToContainTextAsync(new Regex("Admin", RegexOptions.IgnoreCase),
            new LocatorAssertionsToContainTextOptions { Timeout = 10000 });
    }
}
