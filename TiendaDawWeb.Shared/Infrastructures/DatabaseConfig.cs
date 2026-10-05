using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TiendaDawWeb.Shared.Data;

namespace TiendaDawWeb.Shared.Web.Infrastructures;

/// <summary>
/// Configuración de base de datos.
/// Desarrollo: SQLite (archivo temporal o In-Memory).
/// Producción: PostgreSQL (requiere connection string en configuration).
/// </summary>
public static class DatabaseConfig
{
    /// <summary>
    /// Configura la base de datos según el entorno.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <param name="configuration">Configuración de la aplicación.</param>
    /// <param name="environment">Entorno actual (Development/Production).</param>
    /// <returns>IServiceCollection.</returns>
    public static IServiceCollection AddDatabases(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var isE2ETest = Environment.GetEnvironmentVariable("E2E_TEST") == "true";

        if (environment.IsDevelopment() && !isE2ETest)
        {
            // 🎓 Desarrollo: SQLite In-Memory o archivo temporal
            var dbPath = Path.Combine(Path.GetTempPath(), "tiendadb_dev.db");
            if (File.Exists(dbPath)) File.Delete(dbPath);

            var connectionString = $"Data Source={dbPath}";
            Log.Information("🗄️ Desarrollo: SQLite en archivo temporal: {DbPath}", dbPath);

            var keepAliveConnection = new SqliteConnection(connectionString);
            keepAliveConnection.Open();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(keepAliveConnection));
            services.AddSingleton(keepAliveConnection);
        }
        else if (isE2ETest)
        {
            // 🎓 Tests E2E: SQLite en archivo temporal (por puerto)
            var port = Environment.GetEnvironmentVariable("SERVER_PORT") ?? "5000";
            var dbPath = Path.Combine(Path.GetTempPath(), $"tiendadb_{port}.db");
            if (File.Exists(dbPath)) File.Delete(dbPath);

            var connectionString = $"Data Source={dbPath}";
            Log.Information("🗄️ E2E Tests: SQLite en archivo temporal: {DbPath}", dbPath);

            var keepAliveConnection = new SqliteConnection(connectionString);
            keepAliveConnection.Open();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(keepAliveConnection));
            services.AddSingleton(keepAliveConnection);
        }
        else
        {
            // 🛡️ Producción: PostgreSQL — connection string obligatoria
            var connectionString = configuration.GetConnectionString("PostgreSQL")
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
