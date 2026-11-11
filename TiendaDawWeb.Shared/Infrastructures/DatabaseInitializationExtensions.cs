using TiendaDawWeb.Shared.Data;

namespace TiendaDawWeb.Shared.Web.Infrastructures;

public class DatabaseInitializer { }

/// <summary>
///     Extension methods para inicialización de base de datos.
/// </summary>
public static class DatabaseInitializationExtensions {
    /// <summary>
    ///     Inicializa la base de datos SQLite In-Memory.
    /// </summary>
    public static async Task InitializeDatabaseAsync(this WebApplication app, bool isDevelopment) {
        Log.Information("🗄️ Inicializando base de datos...");

        using IServiceScope scope = app.Services.CreateScope();
        ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        ILogger<DatabaseInitializer> logger = scope.ServiceProvider.GetRequiredService<ILogger<DatabaseInitializer>>();

        await context.Database.EnsureCreatedAsync();

        // EnsureCreated no toca una BD que ya existe: si el modelo creció (p. ej.
        // la tabla de passkeys), el parche completa el esquema antes de sembrar.
        await IdentitySchemaPatch.CompletarAsync(context);

        await SeedData.InitializeAsync(scope.ServiceProvider);
        Log.Information("✅ Base de datos inicializada correctamente");
    }
}
