using Microsoft.Playwright;

namespace TiendaDawWeb.Tests.E2E.Security;

/**
 * MÓDULO DE CABECERAS DE SEGURIDAD (E2E)
 *
 * OBJETIVO: Validar que la respuesta HTTP lleva Content-Security-Policy
 * (la protección real contra XSS) y que ya NO envía X-XSS-Protection,
 * que es una cabecera obsoleta que los navegadores modernos ignoran.
 */
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class SecurityHeadersTests : E2ETestBase {
    [Test]
    public async Task Home_LlevaContentSecurityPolicy() {
        IResponse? respuesta = await Page.GotoAsync($"{BaseTestUrl}/");

        Assert.That(respuesta, Is.Not.Null);
        Assert.That(respuesta!.Status, Is.EqualTo(200));

        string? csp = respuesta.Headers.GetValueOrDefault("content-security-policy");
        Assert.That(csp, Is.Not.Null, "falta Content-Security-Policy");
        Assert.That(csp, Does.Contain("default-src 'self'"));
        Assert.That(csp, Does.Contain("frame-ancestors 'none'"));
    }

    [Test]
    public async Task Home_NoEnviaXssProtection() {
        IResponse? respuesta = await Page.GotoAsync($"{BaseTestUrl}/");

        Assert.That(respuesta, Is.Not.Null);
        Assert.That(respuesta!.Headers.ContainsKey("x-xss-protection"), Is.False,
            "X-XSS-Protection es obsoleta y no debe enviarse");
    }
}
