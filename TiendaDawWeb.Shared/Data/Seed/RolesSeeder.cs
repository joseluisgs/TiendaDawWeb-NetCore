namespace TiendaDawWeb.Shared.Data.Seed;

/// <summary>
///     Siembra los roles de Identity (ADMIN, MODERATOR, USER).
///     🎓 Es el primer eslabón del seed: los usuarios no pueden recibir un rol que
///     aún no existe (AddToRoleAsync lanza excepción), así que siempre va primero.
///     Idempotente: si un rol ya está, no lo vuelve a crear.
/// </summary>
public class RolesSeeder(
    RoleManager<IdentityRole<long>> roleManager,
    ILogger<RolesSeeder> logger) {
    /// <summary>Roles que usa la aplicación.</summary>
    public static readonly string[] Roles = ["ADMIN", "MODERATOR", "USER"];

    /// <summary>Crea los roles que falten. No toca los que ya existen.</summary>
    public async Task SeedAsync() {
        foreach (string roleName in Roles) {
            if (await roleManager.RoleExistsAsync(roleName)) {
                continue;
            }

            IdentityResult result = await roleManager.CreateAsync(new IdentityRole<long>(roleName));
            if (result.Succeeded) {
                logger.LogInformation("Rol {Role} creado", roleName);
            } else {
                logger.LogError("Error al crear rol {Role}: {Errors}",
                    roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
