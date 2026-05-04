using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TiendaDawWeb.Shared.Services.Carrito;
using TiendaDawWeb.Shared.Services.Email;
using TiendaDawWeb.Shared.Services.Favorite;
using TiendaDawWeb.Shared.Services.Pdf;
using TiendaDawWeb.Shared.Services.Product;
using TiendaDawWeb.Shared.Services.Purchase;
using TiendaDawWeb.Shared.Services.Rating;
using TiendaDawWeb.Shared.Services.Storage;
using TiendaDawWeb.Shared.Services.BackgroundServices;
using TiendaDawWeb.Shared.Services.Stats;
using TiendaDawWeb.Shared.Infrastructures;

namespace TiendaDawWeb.Shared.Web.Infrastructures;

/// <summary>
/// Extensiones de configuración de servicios de negocio.
/// </summary>
public static class ServicesConfig
{
    /// <summary>
    /// Registra todos los servicios de negocio en el contenedor de dependencias.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        Log.Information("⚙️ Registrando servicios de negocio...");
        return services
            .AddScoped<IProductService, ProductService>()
            .AddScoped<IFavoriteService, FavoriteService>()
            .AddScoped<IStorageService, StorageService>()
            .AddScoped<ICarritoService, CarritoService>()
            .AddScoped<IPurchaseService, PurchaseService>()
            .AddScoped<IRatingService, RatingService>()
            .AddScoped<IEmailService, EmailService>()
            .AddScoped<IPdfService, PdfService>()
            .AddScoped<IStatisticsService, StatisticsService>()
            .AddStateStores();
    }

    /// <summary>
    /// Configura las tareas programadas de background jobs.
    /// </summary>
    public static IServiceCollection AddBackgroundJobs(this IServiceCollection services)
    {
        Log.Information("📰 Configurando Background Jobs...");
        services.AddScoped<ProductoReportTask>();
        return services.AddHostedService<BackgroundJobService>();
    }

    /// <summary>
    /// Configura los servicios de limpieza automática.
    /// </summary>
    public static IServiceCollection AddCleanupServices(this IServiceCollection services)
    {
        Log.Information("🧹 Configurando servicios de limpieza...");
        services.AddHostedService<CarritoCleanupService>();
        return services.AddHostedService<ReservaCleanupService>();
    }
}
