using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TiendaDawWeb.Web.Hubs;

namespace TiendaDawWeb.Web.Infrastructures;

/// <summary>
/// Configuración de SignalR para notificaciones en tiempo real.
/// </summary>
public static class SignalRExtensions
{
    /// <summary>
    /// Configura SignalR con el NotificationHub.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <returns>IServiceCollection.</returns>
    public static IServiceCollection AddAppSignalR(this IServiceCollection services)
    {
        Log.Information("🔔 Configurando SignalR...");

        services.AddSignalR()
            .AddHubOptions<NotificationHub>(options =>
            {
                options.EnableDetailedErrors = true;
                options.MaximumReceiveMessageSize = 1024 * 4;
                options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            });

        Log.Information("🔔 SignalR configurado");

        return services;
    }
}
