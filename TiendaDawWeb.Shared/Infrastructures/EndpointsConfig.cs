using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Serilog;
using TiendaDawWeb.Shared.Web.Hubs;

namespace TiendaDawWeb.Shared.Web.Infrastructures;

/// <summary>
/// Configuración de endpoints de la aplicación.
/// </summary>
public static class EndpointsConfig
{
    /// <summary>
    /// Configura los endpoints MVC y Blazor.
    /// </summary>
    /// <param name="app">Application builder.</param>
    /// <returns>IApplicationBuilder.</returns>
    public static Microsoft.AspNetCore.Builder.IApplicationBuilder MapAppEndpoints(this Microsoft.AspNetCore.Builder.IApplicationBuilder app)
    {
        var webApp = (Microsoft.AspNetCore.Builder.WebApplication)app;

        Log.Information("🛣️ Mapeando endpoints MVC...");
        webApp.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        Log.Information("🛣️ Mapeando Razor Pages...");
        webApp.MapRazorPages();

        Log.Information("🛣️ Mapeando Blazor Hub...");
        webApp.MapBlazorHub();

        Log.Information("🛣️ Mapeando SignalR Hub...");
        webApp.MapHub<NotificationHub>("/notificationHub");

        Log.Information("🛣️ Mapeando Health Check...");
        webApp.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

        return app;
    }
}
