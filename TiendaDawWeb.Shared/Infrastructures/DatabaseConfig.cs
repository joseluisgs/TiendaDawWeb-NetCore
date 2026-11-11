using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using TiendaDawWeb.Shared.Data;

namespace TiendaDawWeb.Shared.Web.Infrastructures;

/// <summary>
///     Configuración de base de datos.
///     Desarrollo: SQLite (archivo temporal o In-Memory).
///     Producción: PostgreSQL (requiere connection string en configuration).
/// </summary>
public static class DatabaseConfig {
    /// <summary>
    ///     Configura la base de datos según el entorno.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <param name="configuration">Configuración de la aplicación.</param>
    /// <param name="environment">Entorno actual (Development/Production).</param>
    /// <returns>IServiceCollection.</returns>
    public static IServiceCollection AddDatabases(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment) {
        bool isE2ETest = Environment.GetEnvironmentVariable("E2E_TEST") == "true";
        bool isExplicitProduction = environment.EnvironmentName == "Production";

        // 🛡️ Solo Production explícito usa PostgreSQL/Redis.
        // Si no se especifica entorno (o es Development/Staging), usa SQLite.
        if (!isExplicitProduction && !isE2ETest) {
            // 🎓 Desarrollo: SQLite en archivo temporal
            string dbPath = Path.Combine(Path.GetTempPath(), "tiendadb_dev.db");
            if (File.Exists(dbPath)) {
                File.Delete(dbPath);
            }

            string connectionString = $"Data Source={dbPath}";
            Log.Information("🗄️ Desarrollo: SQLite en archivo temporal: {DbPath}", dbPath);

            var keepAliveConnection = new SqliteConnection(connectionString);
            keepAliveConnection.Open();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(keepAliveConnection));
            services.AddSingleton(keepAliveConnection);
        } else if (isE2ETest) {
            // 🎓 Tests E2E: SQLite en archivo temporal (por puerto)
            string port = Environment.GetEnvironmentVariable("SERVER_PORT") ?? "5000";
            string dbPath = Path.Combine(Path.GetTempPath(), $"tiendadb_{port}.db");
            if (File.Exists(dbPath)) {
                File.Delete(dbPath);
            }

            string connectionString = $"Data Source={dbPath}";
            Log.Information("🗄️ E2E Tests: SQLite en archivo temporal: {DbPath}", dbPath);

            var keepAliveConnection = new SqliteConnection(connectionString);
            keepAliveConnection.Open();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(keepAliveConnection));
            services.AddSingleton(keepAliveConnection);
        } else {
            // 🛡️ Producción: PostgreSQL — connection string obligatoria
            string connectionString = configuration.GetConnectionString("PostgreSQL")
                                      ?? throw new InvalidOperationException(
                                          "PostgreSQL connection string es obligatoria en producción. " +
                                          "Configura ConnectionStrings:PostgreSQL en appsettings.json.");

            Log.Information("🗄️ Producción: PostgreSQL configurado");
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));
        }

        return services;
    }
}
