using TiendaDawWeb.Shared.Data.Seed;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace TiendaDawWeb.Shared.Data;

/// <summary>
///     Fachada de siembra de datos de ejemplo.
///     🎓 Orquestador, no almacén: los datos viven cada uno en su seeder
///     (<see cref="Seed.RolesSeeder" />, <see cref="Seed.UsersSeeder" />,
///     <see cref="Seed.ProductsSeeder" />) y aquí solo se ejecutan en el orden
///     correcto — roles primero, porque los usuarios no pueden recibir un rol que
///     aún no existe, y productos al final, porque necesitan usuarios.
/// </summary>
public static class SeedData {
    /// <summary>
    ///     Siembra roles, usuarios y catálogo de productos si faltan.
    ///     Idempotente: una BD ya poblada no se modifica.
    /// </summary>
    public static async Task InitializeAsync(IServiceProvider serviceProvider) {
        ILogger logger = serviceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(typeof(SeedData));

        logger.LogInformation("PERFIL DEV: Inicializando marketplace con datos de prueba...");

        // Orden importa: roles → usuarios → productos.
        await serviceProvider.GetRequiredService<RolesSeeder>().SeedAsync();
        await serviceProvider.GetRequiredService<UsersSeeder>().SeedAsync();
        await serviceProvider.GetRequiredService<ProductsSeeder>().SeedAsync();

        logger.LogInformation("Datos de prueba listos: {Usuarios} usuarios, {Productos} productos",
            SeedCredentials.Usuarios.Count, ProductsSeeder.Total);
    }
}
