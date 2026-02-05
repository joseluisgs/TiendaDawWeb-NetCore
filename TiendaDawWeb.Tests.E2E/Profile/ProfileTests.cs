using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using TiendaDawWeb.Tests.E2E.Extensions;

namespace TiendaDawWeb.Tests.E2E.Profile;

/**
 * MÓDULO DE PERFIL (E2E) - Framework Agnóstico
 */
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ProfileTests : E2ETestBase
{
    [SetUp]
    public async Task Setup()
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

    [Test]
    public async Task ProfileDisplay_ShouldShowCorrectUserData()
    {
        await Page.GotoAsync($"{BaseTestUrl}/app/perfil", new() { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 15000 });
        await CaptureScreenshotAsync("04-profile-page");

        var body = Page.Locator("body");
        await Expect(body).ToContainTextAsync(new System.Text.RegularExpressions.Regex("Prueba", System.Text.RegularExpressions.RegexOptions.IgnoreCase), new() { Timeout = 10000 });
        await Expect(body).ToContainTextAsync("prueba@prueba.com", new() { Timeout = 5000 });
    }

    [Test]
    public async Task ProfileEditNavigation_ShouldShowFormWithValues()
    {
        await Page.GotoAsync($"{BaseTestUrl}/app/perfil", new() { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 15000 });
        await CaptureScreenshotAsync("05-profile-page");

        var editLink = Page.Locator("a:has-text('Editar'), a:has-text('Edit')");
        await Expect(editLink).ToBeVisibleAsync(new() { Timeout = 10000 });
        await editLink.ClickAsync();
        await CaptureScreenshotAsync("06-edit-form");

        var body = Page.Locator("body");
        await Expect(body).ToContainTextAsync(new System.Text.RegularExpressions.Regex("Prueba", System.Text.RegularExpressions.RegexOptions.IgnoreCase), new() { Timeout = 10000 });
    }
}
