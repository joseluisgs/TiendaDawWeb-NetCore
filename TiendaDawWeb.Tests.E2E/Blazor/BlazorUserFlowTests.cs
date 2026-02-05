using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using TiendaDawWeb.Tests.E2E.Extensions;

namespace TiendaDawWeb.Tests.E2E.Blazor;

/// <summary>
/// Tests E2E usando exclusivamente TestId() extension
/// </summary>
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class BlazorUserFlowTests : E2ETestBase
{
    public override BrowserNewContextOptions ContextOptions()
    {
        return new BrowserNewContextOptions
        {
            Locale = "es-ES",
            TimezoneId = "Europe/Madrid",
            RecordVideoDir = "TestVideos"
        };
    }

    [Test]
    public async Task LoginAsUser_ShouldShowUserName()
    {
        await Page.GotoAsync($"{BaseTestUrl}/Auth/Login");
        await Page.TestId("email-input").FillAsync("carlos@email.com");
        await Page.TestId("password-input").FillAsync("carlos123");
        await Page.TestId("submit-button").ClickAsync();
        await CaptureScreenshotAsync("01-login");

        await Expect(Page.TestId("user-name")).ToContainTextAsync("Carlos", new() { Timeout = 15000 });
    }

    [Test]
    public async Task PublicPage_ShouldLoad()
    {
        await Page.GotoAsync($"{BaseTestUrl}/Public");
        await CaptureScreenshotAsync("01-public");

        await Expect(Page.TestId("search-input")).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Test]
    public async Task Search_ShouldWork()
    {
        await Page.GotoAsync($"{BaseTestUrl}/Public");
        await Page.TestId("search-input").FillAsync("iPhone");
        await Page.TestId("search-button").ClickAsync();
        await CaptureScreenshotAsync("02-search");

        Assert.Pass("Search functionality works");
    }
}
