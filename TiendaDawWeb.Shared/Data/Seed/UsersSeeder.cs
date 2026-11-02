using Microsoft.AspNetCore.Identity;
using TiendaDawWeb.Shared.Models;

namespace TiendaDawWeb.Shared.Data.Seed;

/// <summary>
/// Siembra los diez usuarios de prueba con UserManager (hash PBKDF2 de Identity)
/// a partir de <see cref="SeedCredentials"/>.
///
/// 🎓 Idempotente por email: un usuario ya creado no se toca. Los roles deben
/// estar ya sembrados (<see cref="RolesSeeder"/>), porque AddToRoleAsync falla
/// si el rol no existe.
/// </summary>
public class UsersSeeder(
    UserManager<User> userManager,
    ILogger<UsersSeeder> logger)
{
    /// <summary>Crea los usuarios que falten y les asigna su rol.</summary>
    public async Task SeedAsync()
    {
        var creados = 0;

        foreach (var semilla in SeedCredentials.Usuarios)
        {
            if (await userManager.FindByEmailAsync(semilla.Email) is not null)
                continue;

            var usuario = new User
            {
                Nombre = semilla.Nombre,
                Apellidos = semilla.Apellidos,
                Email = semilla.Email,
                UserName = semilla.Email,
                Rol = semilla.Rol,
                Avatar = semilla.Avatar,
                EmailConfirmed = true,
            };

            var result = await userManager.CreateAsync(usuario, semilla.Password);
            if (!result.Succeeded)
            {
                logger.LogError("Error al crear usuario {Email}: {Errors}",
                    semilla.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                continue;
            }

            await userManager.AddToRoleAsync(usuario, semilla.Rol);
            creados++;
        }

        logger.LogInformation("Usuarios creados: {Creados} de {Total}",
            creados, SeedCredentials.Usuarios.Count);
    }
}
