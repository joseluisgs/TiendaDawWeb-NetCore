namespace TiendaDawWeb.Shared.Data.Seed;

/// <summary>
/// Un usuario de prueba completo: credenciales y datos de perfil en un sitio.
/// </summary>
public sealed record UsuarioSeed(
    string Email,
    string Password,
    string Nombre,
    string Apellidos,
    string Rol,
    string Avatar);

/// <summary>
/// 🎓 Credenciales seed de una sola fuente de verdad.
///
/// Por qué una clase y no literales repartidos por el seeder: los mismos
/// usuarios, contraseñas y roles se usan en el seeder, en el mensaje de
/// arranque de cada app (Mvc y RazorPages) y en las pruebas. Si cada sitio
/// tiene su copia, en cuanto se cambie una contraseña en un sitio las demás
/// seguirán con la vieja sin que nadie lo note. Aquí se cambia una vez.
///
/// Requisito de las contraseñas: cumplir la política de Identity
/// (≥8 caracteres, mayúscula y dígito) para que sean válidas al crearlas
/// con UserManager.
/// </summary>
public static class SeedCredentials {
    /// <summary>Usuario administrador (rol ADMIN).</summary>
    public const string AdminEmail = "admin@waladaw.com";

    /// <summary>Contraseña del administrador (cumple la política de Identity, ≥8).</summary>
    public const string AdminPassword = "Admin123!";

    /// <summary>Usuario normal de pruebas (rol USER).</summary>
    public const string PruebaEmail = "prueba@prueba.com";

    /// <summary>Contraseña del usuario de pruebas (cumple la política de Identity, ≥8).</summary>
    public const string PruebaPassword = "Prueba123!";

    /// <summary>
    /// Los diez usuarios de prueba, en el orden en que se siembran.
    /// El catálogo de productos les asigna los productos por email.
    /// </summary>
    public static readonly IReadOnlyList<UsuarioSeed> Usuarios =
    [
        new(AdminEmail, AdminPassword, "Admin", "Administrador", "ADMIN",
            "https://robohash.org/admin?size=200x200&bgset=bg1"),
        new(PruebaEmail, PruebaPassword, "Prueba", "Probando Mucho", "USER",
            "https://robohash.org/prueba?size=200x200&bgset=bg2"),
        new("moderador@waladaw.com", "Moderador1", "Moderador", "User", "MODERATOR",
            "https://api.dicebear.com/7.x/avataaars/svg?seed=moderador&backgroundColor=b6e3f4"),
        new("otro@otro.com", "Otro1234", "Otro", "User", "USER",
            "https://api.dicebear.com/7.x/personas/svg?seed=otro&backgroundColor=c0aede"),
        new("maria@email.com", "Maria123", "María", "García López", "USER",
            "https://api.dicebear.com/7.x/avataaars/svg?seed=maria&backgroundColor=ffd5dc"),
        new("carlos@email.com", "Carlos123", "Carlos", "Rodríguez Pérez", "USER",
            "https://robohash.org/carlos?size=200x200&bgset=any&set=set1"),
        new("ana@email.com", "Ana12345", "Ana", "Martín Sánchez", "USER",
            "https://api.dicebear.com/7.x/adventurer/svg?seed=ana&backgroundColor=ffdfbf"),
        new("david@email.com", "David123", "David", "López Torres", "USER",
            "https://robohash.org/david?size=200x200&bgset=bg1&set=set4"),
        new("laura@email.com", "Laura123", "Laura", "Fernández Ruiz", "USER",
            "https://api.dicebear.com/7.x/big-smile/svg?seed=laura&backgroundColor=d1f2eb"),
        new("javier@email.com", "Javier123", "Javier", "Moreno Silva", "USER",
            "https://robohash.org/javier?size=200x200&bgset=any&set=set3"),
    ];
}
