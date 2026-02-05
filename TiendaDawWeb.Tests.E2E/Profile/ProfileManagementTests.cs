using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using TiendaDawWeb.Tests.E2E.Extensions;

namespace TiendaDawWeb.Tests.E2E.Profile;

/// <summary>
/// Tests E2E para gestión de perfil
/// </summary>
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ProfileManagementTests : E2ETestBase
{
    [SetUp]
    public async Task Setup()
    {
        await Page.GotoAsync($"{BaseTestUrl}/Auth/Login");
        await Page.TestId("email-input").FillAsync("prueba@prueba.com");
        await Page.TestId("password-input").FillAsync("prueba");
        await Page.TestId("submit-button").ClickAsync();
        await Expect(Page.TestId("user-name")).ToContainTextAsync("Prueba", new() { Timeout = 15000 });
    }

    [Test]
    public async Task UserIsLoggedIn()
    {
        await Page.GotoAsync($"{BaseTestUrl}/Public");
        await CaptureScreenshotAsync("01-logged-in");

        await Expect(Page.TestId("user-name")).ToContainTextAsync("Prueba", new() { Timeout = 10000 });
    }
}
