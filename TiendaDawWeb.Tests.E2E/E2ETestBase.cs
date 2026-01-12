using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace TiendaDawWeb.Tests.E2E;

public abstract class E2ETestBase : PageTest
{
    private const string BaseUrl = "http://localhost:5000";
    protected string BaseTestUrl => BaseUrl;

    public override BrowserNewContextOptions ContextOptions()
    {
        return new BrowserNewContextOptions
        {
            Locale = "es-ES",
            TimezoneId = "Europe/Madrid",
            RecordVideoDir = "TestVideos"
        };
    }

    protected async Task CaptureScreenshotAsync(string stepName)
    {
        var screenshotPath = Path.Combine("TestScreenshots", TestContext.CurrentContext.Test.Name);
        Directory.CreateDirectory(screenshotPath);
        await Page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = Path.Combine(screenshotPath, $"{stepName}.png"),
            FullPage = true
        });
    }
}
