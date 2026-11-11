using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TiendaDawWeb.Shared.Data;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Web.Infrastructures;

namespace TiendaDawWeb.Tests.Integration;

/// <summary>
///     Tests del parche de esquema de Identity (IdentitySchemaPatch).
///     🎓 Se ejecuta con Identity real sobre SQLite en memoria (no mocks): se simula
///     una BD de producción creada con el esquema anterior (sin AspNetUserPasskeys),
///     se aplica el parche y se comprueba que se puede guardar y leer una passkey
///     con UserManager de verdad.
/// </summary>
[TestFixture]
public class IdentitySchemaPatchTests {
    [SetUp]
    public void Setup() {
        // SQLite en memoria: la conexión se mantiene abierta; si se cierra,
        // la base de datos desaparece con ella.
        _conexion = new SqliteConnection("DataSource=:memory:");
        _conexion.Open();

        var servicios = new ServiceCollection();
        servicios.AddLogging(b => b.SetMinimumLevel(LogLevel.Warning));
        servicios.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(_conexion));

        // Mismo esquema que AuthenticationConfig: sin Version3 no existe la
        // tabla de passkeys y no habría nada que parchear.
        servicios.AddIdentityCore<User>(options => {
                options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>();

        _provider = servicios.BuildServiceProvider();
        _provider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreated();
    }

    [TearDown]
    public void TearDown() {
        _provider.Dispose();
        _conexion.Dispose();
    }

    private SqliteConnection _conexion = null!;
    private ServiceProvider _provider = null!;

    [Test]
    public async Task Completar_EnBdViejaSinPasskeys_CreaTablaYPermiteGuardarPasskey() {
        ApplicationDbContext context = _provider.GetRequiredService<ApplicationDbContext>();

        // Se simula una BD de producción creada con el esquema anterior.
        await context.Database.ExecuteSqlRawAsync("DROP TABLE \"AspNetUserPasskeys\"");

        await IdentitySchemaPatch.CompletarAsync(context);

        // Round-trip con Identity real: alta de usuario, guardado y lectura.
        using IServiceScope scope = _provider.CreateScope();
        UserManager<User> userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        var user = new User { UserName = "pass@test.com", Email = "pass@test.com" };
        (await userManager.CreateAsync(user, "Prueba123!")).Succeeded.Should().BeTrue();

        var passkey = new UserPasskeyInfo(
            [1, 2, 3, 4],
            [5, 6, 7, 8],
            DateTimeOffset.UtcNow,
            1,
            ["usb"],
            true,
            true,
            false,
            [9],
            [10]);

        (await userManager.AddOrUpdatePasskeyAsync(user, passkey)).Succeeded.Should().BeTrue();

        IList<UserPasskeyInfo> guardadas = await userManager.GetPasskeysAsync(user);
        guardadas.Should().HaveCount(1);
        guardadas[0].CredentialId.Should().Equal(1, 2, 3, 4);
        guardadas[0].IsBackedUp.Should().BeFalse();
    }

    [Test]
    public async Task Completar_CuandoLaTablaYaExiste_NoHaceNada() {
        ApplicationDbContext context = _provider.GetRequiredService<ApplicationDbContext>();

        // En una BD nueva EnsureCreated ya ha creado la tabla: el parche es
        // idempotente y no debe intentar crearla otra vez.
        Func<Task> act = () => IdentitySchemaPatch.CompletarAsync(context);

        await act.Should().NotThrowAsync();
    }

    [Test]
    public async Task Completar_DosVecesSeguidas_NoFalla() {
        ApplicationDbContext context = _provider.GetRequiredService<ApplicationDbContext>();
        await context.Database.ExecuteSqlRawAsync("DROP TABLE \"AspNetUserPasskeys\"");

        await IdentitySchemaPatch.CompletarAsync(context);
        await IdentitySchemaPatch.CompletarAsync(context);

        using IServiceScope scope = _provider.CreateScope();
        UserManager<User> userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = new User { UserName = "dos@test.com", Email = "dos@test.com" };
        (await userManager.CreateAsync(user, "Prueba123!")).Succeeded.Should().BeTrue();

        var passkey = new UserPasskeyInfo(
            [11, 12],
            [13, 14],
            DateTimeOffset.UtcNow,
            1,
            null,
            true,
            true,
            true,
            [15],
            [16]);

        (await userManager.AddOrUpdatePasskeyAsync(user, passkey)).Succeeded.Should().BeTrue();
    }
}
