using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace TiendaDawWeb.Tests.E2E;

/**
 * MÓDULO DE AUTENTICACIÓN (E2E)
 * 
 * OBJETIVO: Validar que el flujo de acceso (Login) sea seguro y funcional desde el navegador.
 * TECNOLOGÍAS TESTEADAS: ASP.NET Core Identity, Playwright Locators, Validaciones DataAnnotations.
 */
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class AuthTests : PageTest
{
    private const string BaseUrl = "http://localhost:5000";

    /// <summary>
    /// Configuración previa a cada test: Navegar a la página de login
    /// </summary>
    [SetUp]
    public async Task Setup()
    {
        await Page.GotoAsync($"{BaseUrl}/Auth/Login");
    }

    /// <summary>
    /// Test: Validación de campos vacíos. Debe detectar errores de servidor.
    /// </summary>
    [Test]
    public async Task EmptyFields_ShouldShowValidationErrors()
    {
        // 1. Acción: Enviar formulario sin datos
        await Page.Locator(".card-body form button[type='submit']").ClickAsync();
        
        // 2. Verificación: Aparecen errores de validación (Summary)
        var errors = Page.Locator(".validation-summary-errors");
        await Expect(errors.First).ToBeVisibleAsync();
    }

    /// <summary>
    /// Test: Login exitoso. El administrador debe entrar correctamente.
    /// </summary>
    [Test]
    public async Task AdminLogin_ShouldSucceed()
    {
        // 1. Acción: Rellenar credenciales de administrador
        await Page.FillAsync("#Email", "admin@waladaw.com");
        await Page.FillAsync("#Password", "admin");
        await Page.ClickAsync(".card-body form button[type='submit']");
        
        // 2. Verificación: Cambio de URL y presencia del nombre en Navbar
        await Expect(Page).Not.ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*/Auth/Login.*"));
        await Expect(Page.Locator(".navbar")).ToContainTextAsync(new System.Text.RegularExpressions.Regex("Admin", System.Text.RegularExpressions.RegexOptions.IgnoreCase));
    }

    /// <summary>
    /// Test: Seguridad. Debe rechazar credenciales incorrectas.
    /// </summary>
    [Test]
    public async Task InvalidCredentials_ShouldShowErrorMessage()
    {
        // 1. Acción: Intentar login con datos falsos
        await Page.FillAsync("#Email", "hacker@maligno.com");
        await Page.FillAsync("#Password", "123456");
        await Page.ClickAsync(".card-body form button[type='submit']");
        
        // 2. Verificación: Mensaje de error específico de Identity
        var errorSummary = Page.Locator(".validation-summary-errors");
        await Expect(errorSummary).ToContainTextAsync(new System.Text.RegularExpressions.Regex("incorrectos", System.Text.RegularExpressions.RegexOptions.IgnoreCase));
    }
}
