using Microsoft.Extensions.Configuration;
using Serilog;

namespace TiendaDawWeb.Shared.Web.Infrastructures;

/// <summary>
/// Configuración de CORS (Cross-Origin Resource Sharing).
/// </summary>
public static class CorsConfig
{
    /// <summary>
    /// Configura la política CORS según el entorno.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <param name="configuration">Configuración de la aplicación.</param>
    /// <param name="isDevelopment">Indica si está en desarrollo.</param>
    /// <returns>IServiceCollection.</returns>
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        Log.Information("🌐 Configurando CORS para {Environment}...", isDevelopment ? "DESARROLLO" : "PRODUCCIÓN");

        return services.AddCors(options =>
        {
            if (isDevelopment)
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
                Log.Information("🌐 CORS: AllowAll (desarrollo)");
            }
            else
            {
                options.AddPolicy("ProductionPolicy", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
                Log.Information("🌐 CORS: ProductionPolicy");
            }
        });
    }
}
