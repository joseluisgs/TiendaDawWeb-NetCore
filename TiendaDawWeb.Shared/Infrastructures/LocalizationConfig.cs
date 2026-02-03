using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace TiendaDawWeb.Shared.Web.Infrastructures;

/// <summary>
/// Configuración de localización y culturas soportadas.
/// </summary>
public static class LocalizationConfig
{
    /// <summary>
    /// Configura la localización con recursos RESX.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <returns>IServiceCollection.</returns>
    public static IServiceCollection AddAppLocalization(this IServiceCollection services)
    {
        Log.Information("🌍 Configurando localización...");

        services.AddLocalization(options => options.ResourcesPath = "Resources");

        Log.Information("🌍 Localización configurada");

        return services;
    }

    /// <summary>
    /// Configura las culturas soportadas y el middleware de localización.
    /// </summary>
    /// <param name="app">Application builder.</param>
    /// <returns>IApplicationBuilder.</returns>
    public static Microsoft.AspNetCore.Builder.IApplicationBuilder UseAppLocalization(this Microsoft.AspNetCore.Builder.IApplicationBuilder app)
    {
        var supportedCultures = new[]
        {
            new CultureInfo("es-ES"),
            new CultureInfo("en-US"),
            new CultureInfo("fr-FR"),
            new CultureInfo("de-DE"),
            new CultureInfo("pt-PT")
        };

        var localizationOptions = new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture("es-ES"),
            SupportedCultures = supportedCultures,
            SupportedUICultures = supportedCultures,
            ApplyCurrentCultureToResponseHeaders = true
        };

        // Configurar QueryStringRequestCultureProvider para usar 'lang' en lugar de 'culture'
        var queryProvider = new QueryStringRequestCultureProvider
        {
            QueryStringKey = "lang",
            UIQueryStringKey = "lang"
        };

        localizationOptions.RequestCultureProviders = new List<IRequestCultureProvider>
        {
            queryProvider,
            new CookieRequestCultureProvider(),
            new AcceptLanguageHeaderRequestCultureProvider()
        };

        app.UseRequestLocalization(localizationOptions);

        Log.Information("🌍 Culturas soportadas: es-ES, en-US, fr-FR, de-DE, pt-PT");

        return app;
    }
}
