using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace TiendaDawWeb.Tests.E2E;

/**
 * MÓDULO DE MANEJO DE ERRORES (E2E)
 * 
 * OBJETIVO: Validar que el sistema gestiona correctamente los recursos inexistentes y errores.
 * TECNOLOGÍAS TESTEADAS: TempData Notifications, Filtros de Acción, Redirecciones MVC.
 */
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ErrorHandlingTests : PageTest
{
    private const string BaseUrl = "http://localhost:5000";

    /// <summary>
    /// Test: Recurso no encontrado. Al acceder a un producto inexistente, debe redirigir y avisar al usuario.
    /// </summary>
    [Test]
    public async Task AccessNonExistentProduct_ShouldRedirectAndShowErrorMessage()
    {
        // 0. Acción: Registrar usuario nuevo para asegurar sesión limpia
        string id = System.Guid.NewGuid().ToString().Substring(0, 8);
        await Page.GotoAsync($"{BaseUrl}/Auth/Register");
        await Page.FillAsync("#Nombre", "ErrorUser");
        await Page.FillAsync("#Apellidos", "Test");
        await Page.FillAsync("#Email", $"error_{id}@test.com");
        await Page.FillAsync("#Password", "Password123!");
        await Page.FillAsync("#ConfirmPassword", "Password123!");
        await Page.Locator(".card-body form button[type='submit']").ClickAsync();
        
        await Expect(Page.Locator(".navbar")).ToContainTextAsync("ErrorUser");

        // 1. Acción: Intentar acceder directamente a un ID de producto que no existe
        await Page.GotoAsync($"{BaseUrl}/Product/Details/999999");

        // 2. Verificación: El sistema debe habernos redirigido al listado de productos
        // Usamos una regex flexible que ignore el querystring si lo hay
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*/Product(/Index)?$"));

        // 3. Verificación: Debe aparecer el mensaje de error ("Producto no encontrado")
        var body = Page.Locator("body");
        await Expect(body).ToContainTextAsync(new System.Text.RegularExpressions.Regex("no encontrado", System.Text.RegularExpressions.RegexOptions.IgnoreCase));
    }
}