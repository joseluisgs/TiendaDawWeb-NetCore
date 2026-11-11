namespace TiendaDawWeb.Shared.Data;

/// <summary>
/// Fachada de siembra de datos de ejemplo.
///
/// 🎓 Orquestador, no almacén: los datos viven cada uno en su seeder
/// (<see cref="Seed.RolesSeeder"/>, <see cref="Seed.UsersSeeder"/>,
/// <see cref="Seed.ProductsSeeder"/>) y aquí solo se ejecutan en el orden
/// correcto — roles primero, porque los usuarios no pueden recibir un rol que
/// aún no existe, y productos al final, porque necesitan usuarios.
/// </summary>
public static class SeedData {
    /// <summary>
    /// Siembra roles, usuarios y catálogo de productos si faltan.
    /// Idempotente: una BD ya poblada no se modifica.
    /// </summary>
    public static async Task InitializeAsync(IServiceProvider serviceProvider) {
        var logger = serviceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(typeof(SeedData));

        logger.LogInformation("PERFIL DEV: Inicializando marketplace con datos de prueba...");

        // Orden importa: roles → usuarios → productos.
        await serviceProvider.GetRequiredService<Seed.RolesSeeder>().SeedAsync();
        await serviceProvider.GetRequiredService<Seed.UsersSeeder>().SeedAsync();
        await serviceProvider.GetRequiredService<Seed.ProductsSeeder>().SeedAsync();

        logger.LogInformation("Datos de prueba listos: {Usuarios} usuarios, {Productos} productos",
            Seed.SeedCredentials.Usuarios.Count, Seed.ProductsSeeder.Total);
    }
}
