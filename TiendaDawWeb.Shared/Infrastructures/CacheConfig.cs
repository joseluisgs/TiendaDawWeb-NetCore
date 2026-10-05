using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using StackExchange.Redis;

namespace TiendaDawWeb.Shared.Web.Infrastructures;

/// <summary>
/// Configuración de caché y sesión.
/// Desarrollo: MemoryCache + DistributedMemoryCache.
/// Producción: Redis (OutputCache + MemoryCache distribuido + Session).
/// </summary>
public static class CacheConfig
{
    /// <summary>
    /// Configura caché y sesión según el entorno.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <param name="configuration">Configuración de la aplicación.</param>
    /// <param name="environment">Entorno actual (Development/Production).</param>
    /// <returns>IServiceCollection.</returns>
    public static IServiceCollection AddCaching(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            // 🎓 Desarrollo: MemoryCache (en memoria, sin distribuir)
            Log.Information("💾 Desarrollo: Configurando MemoryCache...");
            services.AddMemoryCache();
            services.AddOutputCache();

            Log.Information("💾 Desarrollo: Configurando Session con DistributedMemoryCache...");
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
        }
        else
        {
            // 🛡️ Producción: Redis para caché distribuida y sesión
            var redisConnection = configuration.GetConnectionString("Redis")
                ?? throw new InvalidOperationException(
                    "Redis connection string es obligatoria en producción. " +
                    "Configura ConnectionStrings:Redis en appsettings.json.");

            Log.Information("🔴 Producción: Configurando Redis...");
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "TiendaDawWeb:";
            });

            // OutputCache con Redis como backend distribuido
            services.AddOutputCache(options =>
            {
                options.AddBasePolicy(builder =>
                {
                    builder.Expire(TimeSpan.FromSeconds(300));
                });
            });

            // Sesión con Redis
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "TiendaDawWeb:Session:";
            });
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            Log.Information("🔴 Producción: Redis configurado para caché y sesión");
        }

        return services;
    }

    /// <summary>
    /// Aplica el middleware de OutputCache al pipeline.
    /// </summary>
    /// <param name="app">Application builder.</param>
    /// <returns>IApplicationBuilder.</returns>
    public static Microsoft.AspNetCore.Builder.IApplicationBuilder UseOutputCaching(this Microsoft.AspNetCore.Builder.IApplicationBuilder app)
    {
        Log.Information("🗂️ Aplicando middleware de OutputCache...");
        app.UseOutputCache();
        return app;
    }
}
