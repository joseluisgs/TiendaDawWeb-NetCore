using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using TiendaDawWeb.Tests.E2E.Extensions;

namespace TiendaDawWeb.Tests.E2E.Auth;

/**
 * MÓDULO DE AUTENTICACIÓN (E2E)
 * 
 * OBJETIVO: Validar que el flujo de acceso (Login) sea seguro y funcional desde el navegador.
 * TECNOLOGÍAS TESTEADAS: ASP.NET Core Identity, Playwright Locators, Validaciones DataAnnotations.
 * 
 * EJEMPLO DE USO DE TEST IDS:
 * - Page.TestId("login-form") - Busca el formulario por data-testid
 * - Page.TestId("email-input").FillAsync("admin@waladaw.com") - Busca input y escribe
 * - Page.TestId("submit-button").ClickAsync() - Busca botón y hace click
 */
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class AuthTests : E2ETestBase
{
    private const string BaseUrl = "http://localhost:5000";

    [SetUp]
    public async Task Setup()
    {
        await Page.GotoAsync($"{BaseUrl}/Auth/Login");
        await CaptureScreenshotAsync("01-login-page-loaded");
    }

    [Test]
    public async Task EmptyFields_ShouldShowValidationErrors()
    {
        await Page.TestId("submit-button").ClickAsync();
        await CaptureScreenshotAsync("02-validation-errors");

        var error = Page.Locator(".text-danger:not(:empty), .field-validation-error").First;
        await Expect(error).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = 10000 });
    }

    [Test]
    public async Task AdminLogin_ShouldSucceed()
    {
        await Page.TestId("email-input").FillAsync("admin@waladaw.com");
        await Page.TestId("password-input").FillAsync("admin");
        await CaptureScreenshotAsync("03-credentials-filled");
        
        await Page.TestId("submit-button").ClickAsync();
        await CaptureScreenshotAsync("04-after-login");
        
        await Expect(Page).Not.ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*/Auth/Login.*"));
        await Expect(Page.Locator(".navbar")).ToContainTextAsync(new System.Text.RegularExpressions.Regex("Admin", System.Text.RegularExpressions.RegexOptions.IgnoreCase));
    }

    [Test]
    public async Task InvalidCredentials_ShouldShowErrorMessage()
    {
        await Page.TestId("email-input").FillAsync("hacker@maligno.com");
        await Page.TestId("password-input").FillAsync("123456");
        await CaptureScreenshotAsync("05-invalid-credentials");
        
        await Page.TestId("submit-button").ClickAsync();
        await CaptureScreenshotAsync("06-error-message");
        
        var errorSummary = Page.Locator(".validation-summary-errors");
        await Expect(errorSummary).ToContainTextAsync(new System.Text.RegularExpressions.Regex("incorrectos", System.Text.RegularExpressions.RegexOptions.IgnoreCase));
    }
}
