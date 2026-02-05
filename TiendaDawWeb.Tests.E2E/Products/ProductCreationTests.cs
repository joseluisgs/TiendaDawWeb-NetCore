using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using TiendaDawWeb.Tests.E2E.Extensions;

namespace TiendaDawWeb.Tests.E2E.Products;

/**
 * MÓDULO DE CREACIÓN DE PRODUCTOS (E2E)
 *
 * OBJETIVO: Verificar el flujo completo de creación de productos.
 * FUNCIONA CON: MVC y Razor Pages indistintamente.
 */
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ProductCreationTests : E2ETestBase
{
    [SetUp]
    public async Task Setup()
    {
        await Page.GotoAsync($"{BaseTestUrl}/Auth/Login");
    }

    [Test]
    public async Task UserCanCreateProduct()
    {
        await Page.TestId("email-input").FillAsync("prueba@prueba.com");
        await Page.TestId("password-input").FillAsync("prueba");
        await Page.TestId("submit-button").ClickAsync();

        await Page.GotoAsync($"{BaseTestUrl}/Product/Create");

        var productName = $"Producto Test {DateTime.UtcNow:yyyyMMddHHmmss}";
        await Page.TestId("nombre-input").FillAsync(productName);
        await Page.TestId("descripcion-input").FillAsync("Descripción de prueba para test E2E");
        await Page.TestId("precio-input").FillAsync("99.99");

        await Page.TestId("submit-button").ClickAsync();

        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*Product/Details.*", System.Text.RegularExpressions.RegexOptions.IgnoreCase), new() { Timeout = 10000 });

        await Expect(Page.Locator("h1").First).ToContainTextAsync(new System.Text.RegularExpressions.Regex(productName, System.Text.RegularExpressions.RegexOptions.IgnoreCase), new() { Timeout = 10000 });
    }

    [Test]
    public async Task ProductOwnerCannotRateOwnProduct()
    {
        await Page.TestId("email-input").FillAsync("prueba@prueba.com");
        await Page.TestId("password-input").FillAsync("prueba");
        await Page.TestId("submit-button").ClickAsync();

        await Page.GotoAsync($"{BaseTestUrl}/Product/Create");
        await Page.TestId("nombre-input").FillAsync($"Mi Producto {DateTime.UtcNow:yyyyMMddHHmmss}");
        await Page.TestId("descripcion-input").FillAsync("Producto para probar");
        await Page.TestId("precio-input").FillAsync("75.00");
        await Page.TestId("submit-button").ClickAsync();

        var url = Page.Url;
        var productId = System.Text.RegularExpressions.Regex.Match(url, @"id=(\d+)").Groups[1].Value;

        await Page.GotoAsync($"{BaseTestUrl}/Product/Details/{productId}");

        var ratingForm = Page.Locator("form:has-text('Valorar')");
        await Expect(ratingForm).Not.ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    [Test]
    public async Task CreatedProductAppearsInPublicList()
    {
        await Page.TestId("email-input").FillAsync("prueba@prueba.com");
        await Page.TestId("password-input").FillAsync("prueba");
        await Page.TestId("submit-button").ClickAsync();

        var productName = $"Buscable {DateTime.UtcNow:yyyyMMddHHmmss}";
        await Page.GotoAsync($"{BaseTestUrl}/Product/Create");
        await Page.TestId("nombre-input").FillAsync(productName);
        await Page.TestId("descripcion-input").FillAsync("Para verificar que aparece");
        await Page.TestId("precio-input").FillAsync("49.99");
        await Page.TestId("submit-button").ClickAsync();

        await Page.GotoAsync($"{BaseTestUrl}/Public");
        await Page.TestId("search-input").FillAsync(productName);
        await Page.TestId("search-button").ClickAsync();

        var productCard = Page.TestId("product-card").First;
        await Expect(productCard).ToBeVisibleAsync(new() { Timeout = 10000 });
    }
}
