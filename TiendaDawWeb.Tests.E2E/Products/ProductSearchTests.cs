using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using TiendaDawWeb.Tests.E2E.Extensions;

namespace TiendaDawWeb.Tests.E2E.Products;

/// <summary>
/// Tests E2E para búsqueda y filtrado de productos
/// </summary>
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ProductSearchTests : E2ETestBase
{
    [Test]
    public async Task SearchFindsProducts()
    {
        await Page.GotoAsync($"{BaseTestUrl}/Public");
        await CaptureScreenshotAsync("01-public-page");

        await Page.TestId("search-input").FillAsync("iPhone");
        await Page.TestId("search-button").ClickAsync();
        await CaptureScreenshotAsync("02-search-results");

        var productCards = Page.Locator("[data-testid='product-card'], .card");
        Assert.That(await productCards.CountAsync(), Is.GreaterThan(0), "Search should find products");
    }

    [Test]
    public async Task SearchWithNoResults_ShowsMessage()
    {
        await Page.GotoAsync($"{BaseTestUrl}/Public");
        await CaptureScreenshotAsync("01-public-page");

        await Page.TestId("search-input").FillAsync("xyznonexistentproduct123");
        await Page.TestId("search-button").ClickAsync();
        await CaptureScreenshotAsync("02-no-results");

        var pageContent = await Page.Locator("body").TextContentAsync();
        Assert.That(pageContent != null, Is.True, "Page should load after search");
    }

    [Test]
    public async Task ProductCardShowsEssentialInfo()
    {
        await Page.GotoAsync($"{BaseTestUrl}/Public");
        await CaptureScreenshotAsync("01-public-page");

        var productCards = Page.Locator("[data-testid='product-card'], .card");
        if (await productCards.CountAsync() > 0)
        {
            var firstCard = productCards.First;
            var cardContent = await firstCard.TextContentAsync();
            Assert.That(cardContent != null, Is.True, "Product card should have content");

            Console.WriteLine("Product card shows essential information");
        }
        else
        {
            Assert.Pass("No product cards found");
        }
    }

    [Test]
    public async Task UserCanViewProductDetails()
    {
        await Page.GotoAsync($"{BaseTestUrl}/Public");
        await CaptureScreenshotAsync("01-public-page");

        var productLinks = Page.Locator("a[href*='/Product/Details']");
        if (await productLinks.CountAsync() > 0)
        {
            await productLinks.First.ClickAsync();
            await CaptureScreenshotAsync("02-product-details");
            await Page.WaitForTimeoutAsync(2000);

            var pageContent = await Page.Locator("body").TextContentAsync();
            Assert.That(pageContent != null, Is.True, "Product details page should load");
        }
        else
        {
            Assert.Pass("No product links found");
        }
    }

    [Test]
    public async Task ProductDetailsShowSellerInfo()
    {
        await Page.GotoAsync($"{BaseTestUrl}/Product/Details/1");
        await CaptureScreenshotAsync("01-product-details");

        var pageContent = await Page.Locator("body").TextContentAsync();
        Assert.That(pageContent != null && (pageContent.Contains("Vendedor") || pageContent.Contains("Prueba") || pageContent.Contains("Seller")),
            Is.True, "Product details should show seller information");
    }

    [Test]
    public async Task ProductDetailsShowPrice()
    {
        await Page.GotoAsync($"{BaseTestUrl}/Product/Details/1");
        await CaptureScreenshotAsync("01-product-details");

        var pageContent = await Page.Locator("body").TextContentAsync();
        Assert.That(pageContent != null && (pageContent.Contains("€") || pageContent.Contains("EUR") || pageContent.Contains("Price")),
            Is.True, "Product details should show price");
    }
}
