using Microsoft.AspNetCore.Http;

namespace TiendaDawWeb.Shared.Web.Infrastructures;

/// <summary>
///     Middleware de headers de seguridad HTTP.
///     Añade headers de protección contra ataques comunes web.
/// </summary>
public class SecurityHeadersMiddleware(RequestDelegate next) {
    private static readonly Dictionary<string, string> SecurityHeaders = new(StringComparer.OrdinalIgnoreCase) {
        ["X-Content-Type-Options"] = "nosniff",
        ["X-Frame-Options"] = "DENY",
        ["Content-Security-Policy"] =
            "default-src 'self'; base-uri 'self'; frame-ancestors 'none'; form-action 'self'; " +
            "script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com; " +
            "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
            "img-src 'self' https: data: blob:; " +
            "font-src 'self' https://cdn.jsdelivr.net data:; " +
            "connect-src 'self'; object-src 'none'",
        ["Referrer-Policy"] = "strict-origin-when-cross-origin",
        ["Permissions-Policy"] =
            "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()"
    };

    private readonly RequestDelegate _next = next;

    /// <summary>
    ///     Añade los headers de seguridad a la respuesta y continúa con la petición.
    ///     X-XSS-Protection no se envía: es una cabecera obsoleta (los navegadores
    ///     modernos la ignoran y OWASP recomienda no enviarla); la protección
    ///     real contra XSS es Content-Security-Policy.
    ///     📌 La política incluye 'unsafe-inline' en script-src de forma transitoria:
    ///     las vistas aún llevan scripts inline (confirmaciones, favoritos, etc.).
    ///     El paso siguiente es externalizarlos a ficheros .js para poder quitarlo.
    /// </summary>
    /// <param name="context">Contexto HTTP de la petición en curso.</param>
    public async Task InvokeAsync(HttpContext context) {
        foreach (KeyValuePair<string, string> header in SecurityHeaders) {
            context.Response.Headers.TryAdd(header.Key, header.Value);
        }

        await _next(context);
    }
}

/// <summary>
///     Extensiones para registro del middleware.
/// </summary>
public static class SecurityHeadersMiddlewareExtensions {
    /// <summary>Registra el middleware de headers de seguridad.</summary>
    /// <param name="app">Constructor de la aplicación.</param>
    /// <returns>IApplicationBuilder.</returns>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app) {
        return app.UseMiddleware<SecurityHeadersMiddleware>();
    }
}
