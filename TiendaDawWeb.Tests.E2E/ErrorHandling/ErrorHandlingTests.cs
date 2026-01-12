using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using TiendaDawWeb.Tests.E2E.Extensions;

namespace TiendaDawWeb.Tests.E2E.ErrorHandling;

/**
 * MÓDULO DE MANEJO DE ERRORES (E2E)
 * 
 * OBJETIVO: Validar que el sistema gestiona correctamente los recursos inexistentes y errores.
 * TECNOLOGÍAS TESTEADAS: StatusCodePages, ExceptionHandler, Redirecciones Públicas.
 */
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ErrorHandlingTests : E2ETestBase
{
    private const string BaseUrl = "http://localhost:5000";

    [Test]
    public async Task AccessNonExistentProduct_ShouldRedirectToPublicWithError()
    {
        await Page.GotoAsync($"{BaseUrl}/Product/Details/999999");
        await CaptureScreenshotAsync("01-not-found-product");

        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*/Public$"));

        await Expect(Page.Locator("body")).ToContainTextAsync(new System.Text.RegularExpressions.Regex("no encontrado", System.Text.RegularExpressions.RegexOptions.IgnoreCase));
    }

    [Test]
    public async Task AccessInvalidRoute_ShouldShowUnifiedErrorPage()
    {
        await Page.GotoAsync($"{BaseUrl}/caca");
        await CaptureScreenshotAsync("02-invalid-route");

        var errorCode = Page.Locator(".error-code");
        await Expect(errorCode).ToContainTextAsync("404");
        
        var errorMessage = Page.Locator(".error-message");
        await Expect(errorMessage).ToContainTextAsync(new System.Text.RegularExpressions.Regex("no existe", System.Text.RegularExpressions.RegexOptions.IgnoreCase));
    }

    [Test]
    public async Task AccessIncompleteRoute_ShouldShowUnifiedErrorPage()
    {
        await Page.GotoAsync($"{BaseUrl}/Product/42");
        await CaptureScreenshotAsync("03-incomplete-route");

        await Expect(Page.Locator(".error-code")).ToContainTextAsync("404");
    }
}
