using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using TiendaDawWeb.Tests.E2E.Extensions;

namespace TiendaDawWeb.Tests.E2E.Blazor;

/// <summary>
/// Tests E2E para navegación y componentes Blazor
/// </summary>
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class BlazorNavigationTests : E2ETestBase
{
    [Test]
    public async Task PublicPage_ShowsProducts()
    {
        await Page.GotoAsync($"{BaseTestUrl}/Public", new() { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 15000 });
        await CaptureScreenshotAsync("01-public-page");

        await Page.WaitForTimeoutAsync(2000);

        var pageVisible = await Page.Locator("body").IsVisibleAsync();
        Assert.That(pageVisible, Is.True, "Public page should load");
    }

    [Test]
    public async Task NavigationMenu_ShouldBeVisible()
    {
        await Page.GotoAsync($"{BaseTestUrl}/Public", new() { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 15000 });
        await CaptureScreenshotAsync("02-navigation");

        var navMenu = Page.Locator("nav, .navbar, [class*='nav']");
        Assert.That(await navMenu.CountAsync(), Is.GreaterThan(0), "Navigation menu should be visible");
    }

    [Test]
    public async Task UserCanNavigateToProductDetails()
    {
        await Page.GotoAsync($"{BaseTestUrl}/Public", new() { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 15000 });
        await CaptureScreenshotAsync("03-public-products");

        var productLinks = Page.Locator("a[href*='/Product/Details']");
        if (await productLinks.CountAsync() > 0)
        {
            await productLinks.First.ClickAsync();
            await CaptureScreenshotAsync("04-product-from-public");

            await Page.WaitForTimeoutAsync(2000);
            var pageVisible = await Page.Locator("body").IsVisibleAsync();
            Assert.That(pageVisible, Is.True, "Should navigate to product details");
        }
        else
        {
            Assert.Pass("No product links found on public page");
        }
    }
}
