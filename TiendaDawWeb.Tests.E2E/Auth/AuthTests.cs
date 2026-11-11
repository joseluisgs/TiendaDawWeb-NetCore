using System.Text.RegularExpressions;
using Microsoft.Playwright;
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
 * - Page.TestId("email-input").FillAsync(AdminEmail) - Busca input y escribe
 * - Page.TestId("submit-button").ClickAsync() - Busca botón y hace click
 */
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class AuthTests : E2ETestBase {
    [SetUp]
    public async Task Setup() {
        await Page.GotoAsync($"{BaseTestUrl}/Auth/Login");
        await CaptureScreenshotAsync("01-login-page-loaded");
    }

    [Test]
    public async Task EmptyFields_ShouldShowValidationErrors() {
        await Page.TestId("submit-button").ClickAsync();
        await CaptureScreenshotAsync("02-validation-errors");

        ILocator error = Page.Locator(".text-danger:not(:empty), .field-validation-error").First;
        await Expect(error).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = 10000 });
    }

    [Test]
    public async Task AdminLogin_ShouldSucceed() {
        await Page.TestId("email-input").FillAsync(AdminEmail);
        await Page.TestId("password-input").FillAsync(AdminPassword);
        await CaptureScreenshotAsync("03-credentials-filled");

        await Page.TestId("submit-button").ClickAsync();
        await CaptureScreenshotAsync("04-after-login");

        await Expect(Page).Not.ToHaveURLAsync(new Regex(".*/Auth/Login.*"));
        await Expect(Page.Locator(".navbar")).ToContainTextAsync(new Regex("Admin", RegexOptions.IgnoreCase));
    }

    [Test]
    public async Task InvalidCredentials_ShouldShowErrorMessage() {
        await Page.TestId("email-input").FillAsync("hacker@maligno.com");
        await Page.TestId("password-input").FillAsync("123456");
        await CaptureScreenshotAsync("05-invalid-credentials");

        await Page.TestId("submit-button").ClickAsync();
        await CaptureScreenshotAsync("06-error-message");

        ILocator errorSummary = Page.Locator(".validation-summary-errors");
        await Expect(errorSummary).ToContainTextAsync(new Regex("incorrectos", RegexOptions.IgnoreCase));
    }
}
