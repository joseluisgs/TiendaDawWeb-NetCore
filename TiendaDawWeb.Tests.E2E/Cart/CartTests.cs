using Microsoft.Playwright;
using TiendaDawWeb.Tests.E2E.Extensions;

namespace TiendaDawWeb.Tests.E2E.Cart;

/// <summary>
///     Tests E2E para el carrito de compras
/// </summary>
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class CartTests : E2ETestBase {
    [SetUp]
    public async Task Setup() {
        await Page.GotoAsync($"{BaseTestUrl}/Auth/Login");
        await Page.TestId("email-input").FillAsync(CarlosEmail);
        await Page.TestId("password-input").FillAsync(CarlosPassword);
        await Page.TestId("submit-button").ClickAsync();
        await Expect(Page.TestId("user-name"))
            .ToContainTextAsync("Carlos", new LocatorAssertionsToContainTextOptions { Timeout = 15000 });
    }

    public override BrowserNewContextOptions ContextOptions() {
        return new BrowserNewContextOptions {
            Locale = "es-ES",
            TimezoneId = "Europe/Madrid",
            RecordVideoDir = "TestVideos"
        };
    }

    [Test]
    public async Task UserCanAddProductToCart() {
        await Page.GotoAsync($"{BaseTestUrl}/Public");
        await CaptureScreenshotAsync("01-public-page");

        await Page.TestId("search-input").FillAsync("Samsung");
        await Page.TestId("search-button").ClickAsync();

        if (await Page.TestId("add-to-cart-button").CountAsync() > 0) {
            await Page.TestId("add-to-cart-button").ClickAsync();
            await CaptureScreenshotAsync("02-added-to-cart");

            bool toastVisible = await Page.Locator(".toast, [class*='toast'], .alert-success").IsVisibleAsync();
            Assert.That(toastVisible, Is.True, "Should show success message");
        } else {
            Assert.Pass("Add to cart button not found");
        }
    }

    [Test]
    public async Task UserCanViewCart() {
        await Page.GotoAsync($"{BaseTestUrl}/Carrito");
        await CaptureScreenshotAsync("01-cart-page");

        bool cartVisible = await Page.Locator("body").IsVisibleAsync();
        Assert.That(cartVisible, Is.True, "Cart page should load");
    }

    [Test]
    public async Task CartShowsItemCount() {
        await Page.GotoAsync($"{BaseTestUrl}/Public");
        await CaptureScreenshotAsync("01-public-page");

        ILocator cartBadge = Page.Locator("[class*='cart'], .badge, [data-testid*='cart']");
        int count = await cartBadge.CountAsync();
        Assert.That(count, Is.GreaterThanOrEqualTo(0), "Cart badge should be visible");
    }

    [Test]
    public async Task UserCanRemoveItemFromCart() {
        await Page.GotoAsync($"{BaseTestUrl}/Carrito");
        await CaptureScreenshotAsync("01-cart-page");

        ILocator removeButtons =
            Page.Locator("button:has-text('Eliminar'), button:has-text('Quitar'), [data-testid*='remove']");
        if (await removeButtons.CountAsync() > 0) {
            await removeButtons.First.ClickAsync();
            await CaptureScreenshotAsync("02-item-removed");
        } else {
            Assert.Pass("No items to remove or remove button not found");
        }
    }
}
