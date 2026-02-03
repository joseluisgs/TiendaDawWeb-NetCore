using Microsoft.AspNetCore.Antiforgery;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace TiendaDawWeb.Shared.Web.Infrastructures;

/// <summary>
/// Configuración de protección CSRF (Antiforgery).
/// </summary>
public static class AntiforgeryConfig
{
    /// <summary>
    /// Configura Antiforgery para protección CSRF.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <returns>IServiceCollection.</returns>
    public static IServiceCollection AddAppAntiforgery(this IServiceCollection services)
    {
        Log.Information("🛡️ Configurando Antiforgery (CSRF)...");

        services.AddAntiforgery(options =>
        {
            options.HeaderName = "RequestVerificationToken";
        });

        Log.Information("🛡️ Antiforgery configurado");

        return services;
    }
}
