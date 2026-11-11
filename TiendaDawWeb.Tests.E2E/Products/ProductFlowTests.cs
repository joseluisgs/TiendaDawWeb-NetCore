using System.Text.RegularExpressions;
using Microsoft.Playwright;
using TiendaDawWeb.Tests.E2E.Extensions;

namespace TiendaDawWeb.Tests.E2E.Products;

/**
 * MÓDULO DE FLUJO DE CATÁLOGO (E2E) - Framework Agnóstico
 *
 * OBJETIVO: Asegurar que el usuario puede encontrar productos mediante el buscador.
 * FUNCIONA CON: MVC y Razor Pages indistintamente.
 */
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ProductFlowTests : E2ETestBase {
    [Test]
    public async Task Search_ShouldFindIphoneAndNavigateToDetails() {
        await Page.GotoAsync($"{BaseTestUrl}/Public");
        await CaptureScreenshotAsync("01-public-page");

        await Page.TestId("search-input").FillAsync("iPhone");
        await Page.TestId("search-button").ClickAsync();
        await CaptureScreenshotAsync("02-search-results");

        ILocator firstProductTitle = Page.TestId("product-title").First;
        await Expect(firstProductTitle).ToContainTextAsync(new Regex("iPhone", RegexOptions.IgnoreCase));

        await firstProductTitle.ClickAsync();
        await CaptureScreenshotAsync("03-product-details");

        await Expect(Page)
            .ToHaveURLAsync(new Regex(".*Product/Details.*id=1|.*Product/Details/1.*", RegexOptions.IgnoreCase),
                new PageAssertionsToHaveURLOptions { Timeout = 10000 });

        ILocator h1 = Page.Locator("h1");
        await Expect(h1).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = 10000 });
        await Expect(h1).ToContainTextAsync(new Regex("iPhone", RegexOptions.IgnoreCase));
    }
}
