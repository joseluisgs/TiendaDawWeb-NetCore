using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using TiendaDawWeb.Tests.E2E.Extensions;

namespace TiendaDawWeb.Tests.E2E.Profile;

/**
 * MÓDULO DE PERFIL (E2E)
 * 
 * OBJETIVO: Verificar que las rutas personalizadas ([Route("app/perfil")]) y la edición funcionan.
 * TECNOLOGÍAS TESTEADAS: Rutas MVC, Atributos de Controlador, Playwright InputValue.
 */
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ProfileTests : E2ETestBase
{
    [SetUp]
    public async Task Setup()
    {
        try
        {
            await Page.GotoAsync($"{BaseTestUrl}/Auth/Login", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
            await CaptureScreenshotAsync("01-login-page");
            
            await Page.TestId("email-input").FillAsync("prueba@prueba.com");
            await Page.TestId("password-input").FillAsync("prueba");
            await CaptureScreenshotAsync("02-credentials-filled");
            
            await Page.TestId("submit-button").ClickAsync();
            await CaptureScreenshotAsync("03-after-login");

            await Expect(Page.TestId("user-name")).ToContainTextAsync(new System.Text.RegularExpressions.Regex("Prueba", System.Text.RegularExpressions.RegexOptions.IgnoreCase), new() { Timeout = 10000 });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR en Setup de ProfileTests: {ex.Message}");
            await Page.ScreenshotAsync(new() { Path = "profile-setup-failure.png" });
            throw;
        }
    }

    [Test]
    public async Task ProfileDisplay_ShouldShowCorrectUserData()
    {
        try
        {
            await Page.GotoAsync($"{BaseTestUrl}/app/perfil", new() { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 15000 });
            await CaptureScreenshotAsync("04-profile-page");

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

    [Test]
    public async Task ProfileEditNavigation_ShouldShowFormWithValues()
    {
        try
        {
            await Page.GotoAsync($"{BaseTestUrl}/app/perfil", new() { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 15000 });
            await CaptureScreenshotAsync("05-profile-page");

            var editLink = Page.Locator("a:has-text('Editar')");
            await Expect(editLink).ToBeVisibleAsync(new() { Timeout = 10000 });
            await editLink.ClickAsync();
            await CaptureScreenshotAsync("06-edit-form");

            await Expect(Page.Locator("#nombre")).ToBeVisibleAsync(new() { Timeout = 10000 });

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
