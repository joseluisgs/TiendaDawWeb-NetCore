using FluentAssertions;
using Microsoft.AspNetCore.Http;
using TiendaDawWeb.Shared.Web.Infrastructures;

namespace TiendaDawWeb.Tests.Middleware;

/// <summary>
///     Tests unitarios del middleware de cabeceras de seguridad.
///     🎓 El contrato: cada respuesta lleva Content-Security-Policy (la protección
///     real contra XSS) y NUNCA X-XSS-Protection, que es una cabecera obsoleta que
///     los navegadores modernos ignoran. La política de la Web permite scripts
///     inline y CDNs porque sus vistas los usan — es una concesión transitoria
///     documentada, no una decisión de seguridad.
/// </summary>
[TestFixture]
public class SecurityHeadersMiddlewareTests {
    private static async Task<DefaultHttpContext> InvocarAsync(string path) {
        var context = new DefaultHttpContext { Request = { Path = path, Method = HttpMethods.Get } };
        context.Response.Body = new MemoryStream();

        var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask);
        await middleware.InvokeAsync(context);

        return context;
    }

    [Test]
    public async Task Invoke_LlevaContentSecurityPolicy() {
        DefaultHttpContext context = await InvocarAsync("/");

        string csp = context.Response.Headers["Content-Security-Policy"].ToString();
        csp.Should().Contain("default-src 'self'", "los recursos por defecto son los del propio origen");
        csp.Should().Contain("frame-ancestors 'none'", "nadie debe embeber la página en un iframe");
        csp.Should().Contain("base-uri 'self'", "sin <base> remoto");
        csp.Should().Contain("object-src 'none'", "sin plugins (Flash, etc.)");
    }

    [Test]
    public async Task Invoke_PoliticaPermiteLosRecursosQueLaWebUsaDeVerdad() {
        DefaultHttpContext context = await InvocarAsync("/");

        string csp = context.Response.Headers["Content-Security-Policy"].ToString();
        // Las vistas llevan scripts inline y cargan CDNs: sin esto se rompería la app.
        csp.Should().Contain("'unsafe-inline'", "transitorio: hasta externalizar los scripts inline");
        csp.Should().Contain("https://cdn.jsdelivr.net", "bootstrap, icons y jquery");
        csp.Should().Contain("https://cdnjs.cloudflare.com", "cliente de SignalR");
        csp.Should().Contain("connect-src 'self'", "SignalR y Blazor Server usan WebSockets del propio origen");
    }

    [Test]
    public async Task Invoke_NoEnviaXssProtection() {
        DefaultHttpContext context = await InvocarAsync("/");

        context.Response.Headers.ContainsKey("X-XSS-Protection").Should().BeFalse(
            "X-XSS-Protection es obsoleta: los navegadores modernos la ignoran y OWASP recomienda no enviarla");
    }

    [Test]
    public async Task Invoke_OtrasCabecerasDeSeguridad_SiguenPresentes() {
        DefaultHttpContext context = await InvocarAsync("/");

        context.Response.Headers["X-Content-Type-Options"].ToString().Should().Be("nosniff");
        context.Response.Headers["X-Frame-Options"].ToString().Should().Be("DENY");
        context.Response.Headers["Referrer-Policy"].ToString().Should().Be("strict-origin-when-cross-origin");
        context.Response.Headers.ContainsKey("Permissions-Policy").Should().BeTrue();
    }
}
