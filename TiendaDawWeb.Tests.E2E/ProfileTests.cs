using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace TiendaDawWeb.Tests.E2E;

/**
 * MÓDULO DE PERFIL (E2E)
 * 
 * OBJETIVO: Verificar que las rutas personalizadas ([Route("app/perfil")]) y la edición funcionan.
 * TECNOLOGÍAS TESTEADAS: Rutas MVC, Atributos de Controlador, Playwright InputValue.
 */
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ProfileTests : PageTest
{
    private const string BaseUrl = "http://localhost:5000";

    /// <summary>
    /// Configuración: Asegurar que el usuario está logueado para ver su perfil.
    /// </summary>
    [SetUp]
    public async Task Setup()
    {
        try
        {
            await Page.GotoAsync($"{BaseUrl}/Auth/Login", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
            await Page.FillAsync("#Email", "prueba@prueba.com");
            await Page.FillAsync("#Password", "prueba");
            await Page.ClickAsync(".card-body form button[type='submit']");

            // Espera explícita de navegación tras el submit
            // await Page.WaitForLoadStateAsync(LoadState.NetworkIdle); // OPTIMIZADO: No es necesario esperar explícitamente

            await Expect(Page.Locator(".navbar")).ToContainTextAsync(new System.Text.RegularExpressions.Regex("Prueba", System.Text.RegularExpressions.RegexOptions.IgnoreCase), new() { Timeout = 10000 });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR en Setup de ProfileTests: {ex.Message}");
            await Page.ScreenshotAsync(new() { Path = "profile-setup-failure.png" });
            throw;
        }
    }

    /// <summary>
    /// Test: Visualización. Debe mostrar los datos reales del usuario logueado.
    /// </summary>
    [Test]
    public async Task ProfileDisplay_ShouldShowCorrectUserData()
    {
        try
        {
            // 1. Acción: Ir a la ruta personalizada
            await Page.GotoAsync($"{BaseUrl}/app/perfil", new() { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 15000 });

            // 2. Verificación: El contenido contiene los datos del SeedData
            var mainContent = Page.Locator("main");
            await Expect(mainContent).ToBeVisibleAsync(new() { Timeout = 10000 });
            await Expect(mainContent).ToContainTextAsync(new System.Text.RegularExpressions.Regex("Prueba", System.Text.RegularExpressions.RegexOptions.IgnoreCase), new() { Timeout = 5000 });
            await Expect(mainContent).ToContainTextAsync("prueba@prueba.com", new() { Timeout = 5000 });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR en ProfileDisplay: {ex.Message}");
            await Page.ScreenshotAsync(new() { Path = "profile-display-failure.png" });
            throw;
        }
    }

    /// <summary>
    /// Test: Navegación. Debe permitir entrar al modo edición y cargar valores.
    /// </summary>
    [Test]
    public async Task ProfileEditNavigation_ShouldShowFormWithValues()
    {
        try
        {
            // 1. Acción: Ir al perfil y click en Editar
            await Page.GotoAsync($"{BaseUrl}/app/perfil", new() { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 15000 });

            // Esperar a que el enlace de editar esté visible
            var editLink = Page.Locator("a:has-text('Editar')");
            await Expect(editLink).ToBeVisibleAsync(new() { Timeout = 10000 });
            await editLink.ClickAsync();

            // Esperar que el formulario de edición sea visible (más fiable que NetworkIdle)
            await Expect(Page.Locator("#nombre")).ToBeVisibleAsync(new() { Timeout = 10000 });

            // 2. Verificación: URL correcta y formulario precargado
            await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*/app/perfil/editar.*"), new() { Timeout = 10000 });

            var nombreInput = Page.Locator("#nombre");
            await Expect(nombreInput).ToBeVisibleAsync(new() { Timeout = 5000 });
            var actualValue = await nombreInput.InputValueAsync();
            Assert.That(actualValue.ToLower(), Does.Contain("prueba"));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR en ProfileEditNavigation: {ex.Message}");
            await Page.ScreenshotAsync(new() { Path = "profile-edit-failure.png" });
            throw;
        }
    }
}
