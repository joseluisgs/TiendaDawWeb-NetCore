using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TiendaDawWeb.Shared.Data;

namespace TiendaDawWeb.Shared.Web.Infrastructures;

/// <summary>
/// Configuración de base de datos SQLite.
/// </summary>
public static class DatabaseConfig
{
    /// <summary>
    /// Configura SQLite con conexión persistente.
    /// Para tests E2E usa archivo temporal en disco para evitar conflictos de concurrencia.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <param name="connectionString">String de conexión SQLite (opcional).</param>
    /// <returns>IServiceCollection.</returns>
    public static IServiceCollection AddDatabases(this IServiceCollection services, string connectionString = "")
    {
        var isE2ETest = Environment.GetEnvironmentVariable("E2E_TEST") == "true";
        var port = Environment.GetEnvironmentVariable("SERVER_PORT") ?? "5000";

        if (isE2ETest)
        {
            var dbPath = Path.Combine(Path.GetTempPath(), $"tiendadb_{port}.db");
            if (File.Exists(dbPath)) File.Delete(dbPath);

            connectionString = $"Data Source={dbPath}";
            Log.Information("🗄️ Configurando SQLite en archivo temporal: {DbPath}", dbPath);
        }
        else
        {
            if (string.IsNullOrEmpty(connectionString))
                connectionString = "DataSource=:memory:";
            Log.Information("🗄️ Configurando SQLite In-Memory...");
        }

        var keepAliveConnection = new SqliteConnection(connectionString);
        keepAliveConnection.Open();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(keepAliveConnection));

        services.AddSingleton(keepAliveConnection);

        Log.Information("🗄️ SQLite configurado correctamente");

        return services;
    }
}
