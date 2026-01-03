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
        await Page.GotoAsync($"{BaseUrl}/Auth/Login");
        await Page.FillAsync("#Email", "prueba@prueba.com");
        await Page.FillAsync("#Password", "prueba");
        await Page.ClickAsync(".card-body form button[type='submit']");
        
        await Expect(Page.Locator(".navbar")).ToContainTextAsync(new System.Text.RegularExpressions.Regex("Prueba", System.Text.RegularExpressions.RegexOptions.IgnoreCase));
    }

    /// <summary>
    /// Test: Visualización. Debe mostrar los datos reales del usuario logueado.
    /// </summary>
    [Test]
    public async Task ProfileDisplay_ShouldShowCorrectUserData()
    {
        // 1. Acción: Ir a la ruta personalizada
        await Page.GotoAsync($"{BaseUrl}/app/perfil");

        // 2. Verificación: El contenido contiene los datos del SeedData
        var mainContent = Page.Locator("main");
        await Expect(mainContent).ToContainTextAsync(new System.Text.RegularExpressions.Regex("Prueba", System.Text.RegularExpressions.RegexOptions.IgnoreCase));
        await Expect(mainContent).ToContainTextAsync("prueba@prueba.com");
    }

    /// <summary>
    /// Test: Navegación. Debe permitir entrar al modo edición y cargar valores.
    /// </summary>
    [Test]
    public async Task ProfileEditNavigation_ShouldShowFormWithValues()
    {
        // 1. Acción: Ir al perfil y click en Editar
        await Page.GotoAsync($"{BaseUrl}/app/perfil");
        await Page.Locator("a:has-text('Editar')").ClickAsync();

        // 2. Verificación: URL correcta y formulario precargado
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*/app/perfil/editar.*"));
        
        var nombreInput = Page.Locator("#nombre");
        var actualValue = await nombreInput.InputValueAsync();
        Assert.That(actualValue.ToLower(), Does.Contain("prueba"));
    }
}
