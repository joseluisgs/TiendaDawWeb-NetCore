using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TiendaDawWeb.Binders;

namespace TiendaDawWeb.Web.Infrastructures;

/// <summary>
/// Extensiones de configuración de MVC, Razor Pages y Blazor Server.
/// </summary>
public static class MvcConfig
{
    /// <summary>
    /// Configura los controladores MVC con binders personalizados.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <returns>IServiceCollection.</returns>
    public static IServiceCollection AddMvcControllers(this IServiceCollection services)
    {
        Log.Information("🎛️ Configurando controladores MVC...");
        services.AddControllersWithViews(options =>
        {
            options.ModelBinderProviders.Insert(0, new DecimalModelBinderProvider());
        })
        .AddViewLocalization()
        .AddDataAnnotationsLocalization();

        return services;
    }

    /// <summary>
    /// Configura Razor Pages.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <returns>IServiceCollection.</returns>
    public static IServiceCollection AddAppRazorPages(this IServiceCollection services)
    {
        Log.Information("📄 Configurando Razor Pages...");
        services.AddRazorPages();
        return services;
    }

    /// <summary>
    /// Configura Blazor Server con opciones detalladas de errores.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <returns>IServiceCollection.</returns>
    public static IServiceCollection AddBlazorServer(this IServiceCollection services)
    {
        Log.Information("🔵 Configurando Blazor Server...");
        services.AddServerSideBlazor()
            .AddCircuitOptions(options => { options.DetailedErrors = true; });
        return services;
    }
}
