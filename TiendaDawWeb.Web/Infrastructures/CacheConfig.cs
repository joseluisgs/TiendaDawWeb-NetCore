using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace TiendaDawWeb.Web.Infrastructures;

/// <summary>
/// Configuración de caché y sesión.
/// </summary>
public static class CacheConfig
{
    /// <summary>
    /// Configura OutputCache, MemoryCache y Session.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <returns>IServiceCollection.</returns>
    public static IServiceCollection AddCaching(this IServiceCollection services)
    {
        Log.Information("🧠 Configurando OutputCache...");

        services.AddOutputCache();

        Log.Information("🧠 Configurando MemoryCache...");
        services.AddMemoryCache();

        Log.Information("🧠 Configurando Session...");
        services.AddDistributedMemoryCache();
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        Log.Information("🧠 Caché y sesión configurados");

        return services;
    }
}
