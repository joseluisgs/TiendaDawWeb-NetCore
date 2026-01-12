using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using TiendaDawWeb.Tests.E2E.Extensions;

namespace TiendaDawWeb.Tests.E2E.Products;

/**
 * MÓDULO DE FLUJO DE CATÁLOGO (E2E)
 * 
 * OBJETIVO: Asegurar que el usuario puede encontrar productos mediante el buscador.
 * TECNOLOGÍAS TESTEADAS: Motores de búsqueda (q), Vistas Razor, Rutas MVC.
 */
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ProductFlowTests : E2ETestBase
{
    private const string BaseUrl = "http://localhost:5000";

    [Test]
    public async Task Search_ShouldFindIphoneAndNavigateToDetails()
    {
        await Page.GotoAsync($"{BaseUrl}/Public");
        await CaptureScreenshotAsync("01-public-page");

        await Page.TestId("search-input").FillAsync("iPhone");
        await Page.TestId("search-button").ClickAsync();
        await CaptureScreenshotAsync("02-search-results");

        var firstProductTitle = Page.TestId("product-title").First;
        await Expect(firstProductTitle).ToContainTextAsync(new System.Text.RegularExpressions.Regex("iPhone", System.Text.RegularExpressions.RegexOptions.IgnoreCase));

        await firstProductTitle.ClickAsync();
        await CaptureScreenshotAsync("03-product-details");

        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*/Product/Details/.*"));
        var h1 = Page.Locator("h1");
        await Expect(h1).ToBeVisibleAsync();
        await Expect(h1).ToContainTextAsync(new System.Text.RegularExpressions.Regex("iPhone", System.Text.RegularExpressions.RegexOptions.IgnoreCase));
    }
}
