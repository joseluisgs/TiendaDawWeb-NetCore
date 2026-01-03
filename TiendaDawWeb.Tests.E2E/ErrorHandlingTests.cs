using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace TiendaDawWeb.Tests.E2E;

/**
 * MÓDULO DE MANEJO DE ERRORES (E2E)
 * 
 * OBJETIVO: Validar que el sistema gestiona correctamente los recursos inexistentes y errores.
 * TECNOLOGÍAS TESTEADAS: StatusCodePages, ExceptionHandler, Redirecciones Públicas.
 */
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ErrorHandlingTests : PageTest
{
    private const string BaseUrl = "http://localhost:5000";

    /// <summary>
    /// Test: Producto no encontrado. Debe redirigir a /Public (sin login) y mostrar mensaje.
    /// </summary>
    [Test]
    public async Task AccessNonExistentProduct_ShouldRedirectToPublicWithError()
    {
        // 1. Acción: Intentar acceder a un producto falso (sin estar logueado)
        await Page.GotoAsync($"{BaseUrl}/Product/Details/999999");

        // 2. Verificación: Redirige a /Public (no a Login)
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*/Public$"));

        // 3. Verificación: Mensaje de error visible
        await Expect(Page.Locator("body")).ToContainTextAsync(new System.Text.RegularExpressions.Regex("no encontrado", System.Text.RegularExpressions.RegexOptions.IgnoreCase));
    }

    /// <summary>
    /// Test: Ruta inexistente (404). Debe mostrar la página de error unificada.
    /// </summary>
    [Test]
    public async Task AccessInvalidRoute_ShouldShowUnifiedErrorPage()
    {
        // 1. Acción: Ir a una ruta que no existe en los controladores
        await Page.GotoAsync($"{BaseUrl}/caca");

        // 2. Verificación: Se muestra el código 404 y el mensaje personalizado
        var errorCode = Page.Locator(".error-code");
        await Expect(errorCode).ToContainTextAsync("404");
        
        var errorMessage = Page.Locator(".error-message");
        await Expect(errorMessage).ToContainTextAsync(new System.Text.RegularExpressions.Regex("no existe", System.Text.RegularExpressions.RegexOptions.IgnoreCase));
    }

    /// <summary>
    /// Test: Error de ruta parcial (404). Acceder a un controlador sin acción válida.
    /// </summary>
    [Test]
    public async Task AccessIncompleteRoute_ShouldShowUnifiedErrorPage()
    {
        // 1. Acción: Ir a /Product/42 (esto falla porque no hay acción 42)
        await Page.GotoAsync($"{BaseUrl}/Product/42");

        // 2. Verificación: Página 404 unificada
        await Expect(Page.Locator(".error-code")).ToContainTextAsync("404");
    }
}
