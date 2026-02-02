using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TiendaDawWeb.Data;

namespace TiendaDawWeb.Web.Infrastructures;

/// <summary>
/// Configuración de base de datos SQLite en memoria.
/// </summary>
public static class DatabaseConfig
{
    /// <summary>
    /// Configura SQLite con conexión persistente en memoria.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <param name="connectionString">String de conexión SQLite.</param>
    /// <returns>IServiceCollection.</returns>
    public static IServiceCollection AddDatabases(this IServiceCollection services, string connectionString = "DataSource=:memory:")
    {
        Log.Information("🗄️ Configurando SQLite In-Memory...");

        var keepAliveConnection = new SqliteConnection(connectionString);
        keepAliveConnection.Open();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(keepAliveConnection));

        services.AddSingleton(keepAliveConnection);

        Log.Information("🗄️ SQLite configurado correctamente");

        return services;
    }
}
