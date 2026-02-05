using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using System;

namespace TiendaDawWeb.Tests.E2E;

public abstract class E2ETestBase : PageTest
{
    private const string DefaultBaseUrl = "http://localhost:5000";

    protected string BaseTestUrl => Environment.GetEnvironmentVariable("E2E_BASE_URL")
        ?? Environment.GetEnvironmentVariable("ASPNETCORE_URLS")
        ?? DefaultBaseUrl;

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
