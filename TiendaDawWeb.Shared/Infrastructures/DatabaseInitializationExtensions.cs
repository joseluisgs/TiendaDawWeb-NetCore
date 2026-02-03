using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TiendaDawWeb.Shared.Data;

namespace TiendaDawWeb.Shared.Web.Infrastructures;

public class DatabaseInitializer { }

/// <summary>
/// Extension methods para inicialización de base de datos.
/// </summary>
public static class DatabaseInitializationExtensions
{
    /// <summary>
    /// Inicializa la base de datos SQLite In-Memory.
    /// </summary>
    public static async Task InitializeDatabaseAsync(this WebApplication app, bool isDevelopment)
    {
        Log.Information("🗄️ Inicializando base de datos...");

        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DatabaseInitializer>>();

        await context.Database.EnsureCreatedAsync();
        await SeedData.InitializeAsync(scope.ServiceProvider);
        Log.Information("✅ Base de datos inicializada correctamente");
    }
}
