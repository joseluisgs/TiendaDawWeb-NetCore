using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace TiendaDawWeb.Tests.E2E;

public abstract class E2ETestBase : PageTest {
    private const string DefaultBaseUrl = "http://localhost:5000";

    // Credenciales de prueba: el E2E es un cliente externo (solo Playwright,
    // sin referencias a la app), así que mantiene aquí su propia copia. Si
    // cambian en la app, hay que cambiarlas también — el primer test de login
    // lo avisa en rojo.
    protected const string AdminEmail = "admin@waladaw.com";
    protected const string AdminPassword = "Admin123!";
    protected const string PruebaEmail = "prueba@prueba.com";
    protected const string PruebaPassword = "Prueba123!";
    protected const string CarlosEmail = "carlos@email.com";
    protected const string CarlosPassword = "Carlos123";

    protected string BaseTestUrl => Environment.GetEnvironmentVariable("E2E_BASE_URL")
                                    ?? Environment.GetEnvironmentVariable("ASPNETCORE_URLS")
                                    ?? DefaultBaseUrl;

    public override BrowserNewContextOptions ContextOptions() {
        return new BrowserNewContextOptions {
            Locale = "es-ES",
            TimezoneId = "Europe/Madrid",
            RecordVideoDir = "TestVideos"
        };
    }

    protected async Task CaptureScreenshotAsync(string stepName) {
        string screenshotPath = Path.Combine("TestScreenshots", TestContext.CurrentContext.Test.Name);
        Directory.CreateDirectory(screenshotPath);
        await Page.ScreenshotAsync(new PageScreenshotOptions {
            Path = Path.Combine(screenshotPath, $"{stepName}.png"),
            FullPage = true
        });
    }
}
