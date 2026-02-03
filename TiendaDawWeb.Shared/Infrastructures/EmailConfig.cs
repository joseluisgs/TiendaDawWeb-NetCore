using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TiendaDawWeb.Shared.Services.Email;
using TiendaDawWeb.Shared.Services.BackgroundServices;

namespace TiendaDawWeb.Shared.Web.Infrastructures;

/// <summary>
/// Extensiones de configuración de servicios de email.
/// </summary>
public static class EmailConfig
{
    /// <summary>
    /// Configura el servicio de email y su channel.
    /// </summary>
    public static IServiceCollection AddEmail(this IServiceCollection services)
    {
        services.AddSingleton(Channel.CreateUnbounded<EmailMessage>());
        services.AddScoped<IEmailService, EmailService>();
        services.AddHostedService<EmailBackgroundService>();
        return services;
    }
}
