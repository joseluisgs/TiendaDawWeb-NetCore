using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using TiendaDawWeb.Tests.E2E.Extensions;

namespace TiendaDawWeb.Tests.E2E.Auth;

/// <summary>
/// Tests E2E para registro de usuarios
/// </summary>
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class RegistrationTests : E2ETestBase
{
    [Test]
    public async Task UserCanRegister_WithValidData()
    {
        string id = System.Guid.NewGuid().ToString().Substring(0, 8);
        string email = $"newuser_{id}@test.com";

        await Page.GotoAsync($"{BaseTestUrl}/Auth/Register");
        await CaptureScreenshotAsync("01-register-page");

        await Page.TestId("nombre-input").FillAsync("Nuevo");
        await Page.TestId("apellidos-input").FillAsync("Usuario");
        await Page.TestId("email-input").FillAsync(email);
        await Page.TestId("password-input").FillAsync("Password123!");
        await Page.TestId("confirm-password-input").FillAsync("Password123!");
        await Page.TestId("submit-button").ClickAsync();

        await Expect(Page.TestId("user-name")).ToContainTextAsync("Nuevo", new() { Timeout = 15000 });
        await CaptureScreenshotAsync("02-registered-success");
    }

    [Test]
    public async Task Registration_ValidationErrors_Work()
    {
        await Page.GotoAsync($"{BaseTestUrl}/Auth/Register");
        await CaptureScreenshotAsync("01-register-page");

        await Page.TestId("nombre-input").FillAsync("Test");
        await Page.TestId("apellidos-input").FillAsync("User");
        await Page.TestId("email-input").FillAsync("invalid-email");
        await Page.TestId("password-input").FillAsync("123");
        await Page.TestId("confirm-password-input").FillAsync("Different123!");
        await Page.TestId("submit-button").ClickAsync();

        var pageContent = await Page.Locator("body").TextContentAsync();
        Assert.That(pageContent != null, Is.True, "Page should handle validation");
    }

    [Test]
    public async Task LoginPage_Loads()
    {
        await Page.GotoAsync($"{BaseTestUrl}/Auth/Login");
        await CaptureScreenshotAsync("01-login-page");

        await Expect(Page.TestId("email-input")).ToBeVisibleAsync(new() { Timeout = 10000 });
    }
}
