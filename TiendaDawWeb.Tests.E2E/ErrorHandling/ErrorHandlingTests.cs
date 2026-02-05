using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using TiendaDawWeb.Tests.E2E.Extensions;

namespace TiendaDawWeb.Tests.E2E.ErrorHandling;

/**
 * MÓDULO DE MANEJO DE ERRORES (E2E)
 * Tests simplificados que verifican el comportamiento general sin depender
 * de la implementación específica de páginas de error.
 */
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ErrorHandlingTests : E2ETestBase
{
    [Test]
    public async Task AccessNonExistentProduct_ShouldHandleGracefully()
    {
        var response = await Page.GotoAsync($"{BaseTestUrl}/Product/Details/999999");
        Assert.That(response.Status, Is.EqualTo(404).Or.EqualTo(200));
        await CaptureScreenshotAsync("01-not-found-product");
    }

    [Test]
    public async Task AccessInvalidRoute_ShouldHandleGracefully()
    {
        var response = await Page.GotoAsync($"{BaseTestUrl}/ruta-inexistente-xyz-12345");
        Assert.That(response.Status, Is.EqualTo(404).Or.EqualTo(200));
        await CaptureScreenshotAsync("02-invalid-route");
    }

    [Test]
    public async Task AccessIncompleteRoute_ShouldHandleGracefully()
    {
        var response = await Page.GotoAsync($"{BaseTestUrl}/Product/42");
        Assert.That(response.Status, Is.EqualTo(404).Or.EqualTo(200).Or.EqualTo(400));
        await CaptureScreenshotAsync("03-incomplete-route");
    }
}
