using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using TiendaDawWeb.Shared.Data;
using TiendaDawWeb.Shared.Data.Seed;
using TiendaDawWeb.Shared.Models;

namespace TiendaDawWeb.Tests.Seed;

/// <summary>
/// Tests del sistema de siembra unificado (Data/Seed).
///
/// 🎓 Se ejecuta con Identity real sobre SQLite en memoria (no mocks): la
/// siembra usa UserManager de verdad y el objetivo es comprobar que es
/// idempotente — dos corridas seguidas dejan EXACTAMENTE los mismos datos.
/// </summary>
[TestFixture]
public class SeedDataTests
{
    private SqliteConnection _conexion = null!;
    private ServiceProvider _provider = null!;

    [SetUp]
    public void Setup()
    {
        // SQLite en memoria: la conexión se mantiene abierta; si se cierra,
        // la base de datos desaparece con ella.
        _conexion = new SqliteConnection("DataSource=:memory:");
        _conexion.Open();

        var servicios = new ServiceCollection();
        servicios.AddLogging(b => b.SetMinimumLevel(LogLevel.Warning));
        servicios.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(_conexion));

        // Mismas reglas que la aplicación (AuthenticationConfig): con la
        // política por defecto de Identity "Admin1" no sería válida.
        servicios.AddIdentityCore<User>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole<long>>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        servicios.AddScoped<RolesSeeder>();
        servicios.AddScoped<UsersSeeder>();
        servicios.AddScoped<ProductsSeeder>();

        _provider = servicios.BuildServiceProvider();
        _provider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreated();
    }

    [TearDown]
    public void TearDown()
    {
        _provider.Dispose();
        _conexion.Dispose();
    }

    [Test]
    public async Task Initialize_DosVecesSeguidas_NoDuplicaNada()
    {
        await SembrarAsync();
        await SembrarAsync();

        using var scope = _provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        (await context.Users.CountAsync()).Should().Be(SeedCredentials.Usuarios.Count,
            "los usuarios se dan de alta una sola vez");
        (await context.Roles.CountAsync()).Should().Be(RolesSeeder.Roles.Length,
            "los roles se dan de alta una sola vez");
        (await context.Products.CountAsync()).Should().Be(ProductsSeeder.Total,
            "el catálogo no debe duplicarse");
    }

    [Test]
    public async Task Initialize_ConCatalogoParcial_ReponeLosProductosQueFaltan()
    {
        await SembrarAsync();

        // Se borra un producto: el catálogo deja de estar completo.
        using (var scope = _provider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var producto = await context.Products.FirstAsync(p => p.Nombre == "iPhone 17 Pro Max");
            context.Products.Remove(producto);
            await context.SaveChangesAsync();
        }

        await SembrarAsync();

        using (var scope = _provider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            (await context.Products.AnyAsync(p => p.Nombre == "iPhone 17 Pro Max")).Should().BeTrue(
                "el seeder repone lo que falte sin tocar el resto");
            (await context.Products.CountAsync()).Should().Be(ProductsSeeder.Total);
        }
    }

    [Test]
    public async Task Initialize_UsuariosTienenSuRolAsignado()
    {
        await SembrarAsync();

        using var scope = _provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var admin = await context.Users.SingleAsync(u => u.Email == SeedCredentials.AdminEmail);
        var roles = await context.UserRoles
            .Where(ur => ur.UserId == admin.Id)
            .Select(ur => ur.RoleId)
            .ToListAsync();
        var nombres = await context.Roles
            .Where(r => roles.Contains(r.Id))
            .Select(r => r.Name)
            .ToListAsync();

        nombres.Should().ContainSingle().Which.Should().Be("ADMIN");
    }

    private async Task SembrarAsync()
    {
        using var scope = _provider.CreateScope();
        await SeedData.InitializeAsync(scope.ServiceProvider);
    }
}
