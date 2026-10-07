using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog;

namespace TiendaDawWeb.Shared.Middleware;

/// <summary>
/// Middleware que guarda la ruta original de la petición en HttpContext.Items.
/// Se usa en el ErrorController para registrar la ruta en los logs de error.
/// </summary>
public class OriginalPathMiddleware(
    RequestDelegate next,
    ILogger<OriginalPathMiddleware> logger
)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<OriginalPathMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        // Guardar la ruta original antes de que StatusCodePages la re-execute
        context.Items["OriginalPath"] = context.Request.Path + context.Request.QueryString;
        await _next(context);
    }
}

/// <summary>
/// Extensiones para registrar el middleware.
/// </summary>
public static class OriginalPathMiddlewareExtensions
{
    /// <summary>
    /// Registra el middleware que guarda la ruta original.
    /// Debe ir ANTES de UseStatusCodePagesWithReExecute.
    /// </summary>
    public static IApplicationBuilder UseOriginalPath(this IApplicationBuilder app)
    {
        return app.UseMiddleware<OriginalPathMiddleware>();
    }
}
